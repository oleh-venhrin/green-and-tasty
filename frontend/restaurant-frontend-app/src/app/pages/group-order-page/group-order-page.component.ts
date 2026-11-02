import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { Subject, Subscription, timer } from 'rxjs';
import { exhaustMap, filter, takeUntil } from 'rxjs/operators';
import { GroupOrdersService } from '@app/services/group-orders/group-orders.service';
import { ToastService } from '@app/services/toast/toast.service';
import { GuestInvite } from '@app/models/reservations/guest-invite.model';
import { GroupSessionsResponse } from '@app/models/group-orders/group-sessions-response.model';
import { GroupStatusResponse } from '@app/models/group-orders/group-status.model';
import { Participant } from '@app/models/group-orders/participant.model';
import { consolidatePreOrder } from '@app/shared/helpers/consolidate-pre-order';
import { ParticipantCardComponent } from '@app/components/group-orders/participant-card/participant-card.component';
import { ModalComponent } from '@app/shared/custom-components/modal/modal.component';

const SYNC_INTERVAL_MS = 15_000;

@Component({
  selector: 'app-group-order-page',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, ParticipantCardComponent, ModalComponent],
  templateUrl: './group-order-page.component.html',
  styleUrl: './group-order-page.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class GroupOrderPageComponent implements OnInit {
  private readonly service = inject(GroupOrdersService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly reservationId = signal<string>('');
  readonly invite = signal<GuestInvite | null>(null);
  readonly sessions = signal<GroupSessionsResponse | null>(null);
  readonly ownerSessionId = signal<string | null>(null);
  readonly groupStatus = signal<GroupStatusResponse | null>(null);

  readonly loading = signal<boolean>(true);
  readonly errorMessage = signal<string | null>(null);
  readonly is404 = signal<boolean>(false);

  readonly submitting = signal<boolean>(false);
  readonly showSubmitModal = signal<boolean>(false);
  readonly guestToRemove = signal<Participant | null>(null);

  readonly lastUpdated = signal<Date>(new Date());
  readonly syncError = signal<boolean>(false);

  private pollingSub?: Subscription;
  private readonly manualRefresh$ = new Subject<void>();

  readonly participants = computed(() => this.sessions()?.participants ?? []);

  readonly isSubmitted = computed(() => this.groupStatus()?.status === 'Submitted');

  readonly statusBadgeText = computed(() =>
    this.isSubmitted() ? 'Sent to Restaurant' : 'Collecting pre-orders'
  );

  readonly consolidated = computed(() => {
    const list = this.participants();
    return consolidatePreOrder(list);
  });

  readonly grandTotal = computed(() => this.consolidated().total);

  readonly totalItemsCount = computed(() => {
    return this.participants().reduce((acc, p) => acc + (p.items?.length ?? 0), 0);
  });

  readonly unsubmittedCount = computed(() => {
    return this.participants().filter((p) => !p.isSubmitted).length;
  });

  readonly canSubmit = computed(() => {
    if (this.submitting() || this.isSubmitted()) return false;
    return this.consolidated().items.length > 0;
  });

  ngOnInit(): void {
    const resId =
      this.route.snapshot.paramMap.get('reservationId') ||
      this.route.snapshot.params['reservationId'] ||
      '';
    this.reservationId.set(resId);
    this.loadData();

    this.destroyRef.onDestroy(() => {
      this.stopPolling();
    });
  }

  loadData(): void {
    const resId = this.reservationId();
    if (!resId) return;

    this.loading.set(true);
    this.errorMessage.set(null);
    this.is404.set(false);
    this.stopPolling();

    this.service.getGroupInvite$(resId).subscribe({
      next: (inviteData) => {
        this.invite.set(inviteData);
        const groupId = inviteData.GroupReservationId;

        this.service.getOwnerSessionId$(resId).subscribe({
          next: (ownerId) => this.ownerSessionId.set(ownerId),
          error: () => {},
        });

        this.service.getStatus$(groupId).subscribe({
          next: (statusData) => {
            this.groupStatus.set(statusData);
            if (statusData.status !== 'Submitted') {
              this.startPolling(groupId);
            }
          },
          error: () => {},
        });

        this.service.getSessions$(groupId).subscribe({
          next: (sessionsData) => {
            this.sessions.set(sessionsData);
            this.lastUpdated.set(new Date());
            this.loading.set(false);
          },
          error: (err: HttpErrorResponse) => {
            this.handleLoadError(err);
          },
        });
      },
      error: (err: HttpErrorResponse) => {
        this.handleLoadError(err);
      },
    });
  }

  private handleLoadError(err: HttpErrorResponse): void {
    this.loading.set(false);
    if (err.status === 404) {
      this.is404.set(true);
      this.errorMessage.set('Group reservation not found');
    } else {
      this.errorMessage.set('Failed to load group pre-order data. Please try again.');
    }
  }

  private startPolling(groupId: string): void {
    this.stopPolling();

    this.pollingSub = timer(SYNC_INTERVAL_MS, SYNC_INTERVAL_MS)
      .pipe(
        filter(() => !this.isSubmitted()),
        exhaustMap(() => this.service.getSessions$(groupId, { silent: true }))
      )
      .subscribe({
        next: (updatedSessions) => {
          this.sessions.set(updatedSessions);
          this.lastUpdated.set(new Date());
          this.syncError.set(false);
        },
        error: () => {
          this.syncError.set(true);
        },
      });
  }

  stopPolling(): void {
    if (this.pollingSub) {
      this.pollingSub.unsubscribe();
      this.pollingSub = undefined;
    }
  }

  refreshNow(): void {
    const groupId = this.invite()?.GroupReservationId;
    if (!groupId) return;

    this.service.getSessions$(groupId).subscribe({
      next: (updatedSessions) => {
        this.sessions.set(updatedSessions);
        this.lastUpdated.set(new Date());
        this.syncError.set(false);
      },
      error: () => {
        this.syncError.set(true);
      },
    });

    this.service.getStatus$(groupId).subscribe({
      next: (statusData) => {
        this.groupStatus.set(statusData);
        if (statusData.status === 'Submitted') {
          this.stopPolling();
        }
      },
      error: () => {},
    });
  }

  onSubmitPreOrderClick(): void {
    if (!this.canSubmit()) return;
    this.showSubmitModal.set(true);
  }

  confirmSubmit(): void {
    if (this.submitting()) return;

    const groupId = this.invite()?.GroupReservationId;
    const resId = this.reservationId();
    if (!groupId || !resId) return;

    this.submitting.set(true);

    this.service.submit$(groupId, resId).subscribe({
      next: () => {
        this.submitting.set(false);
        this.showSubmitModal.set(false);
        this.toast.success('Pre-order sent to the restaurant');

        const current = this.groupStatus();
        this.groupStatus.set({
          currentParticipants: current?.currentParticipants ?? 0,
          maxCapacity: current?.maxCapacity ?? 0,
          status: 'Submitted',
        });
        this.stopPolling();
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.showSubmitModal.set(false);
        this.toast.error(err.error?.message || 'Failed to submit pre-order');
      },
    });
  }

  onRemoveGuest(participant: Participant): void {
    if (this.isSubmitted()) return;
    this.guestToRemove.set(participant);
  }

  confirmRemoveGuest(): void {
    const guest = this.guestToRemove();
    const groupId = this.invite()?.GroupReservationId;
    if (!guest || !groupId) return;

    this.service.removeSession$(groupId, guest.id).subscribe({
      next: (updatedSessions) => {
        this.sessions.set(updatedSessions);
        this.guestToRemove.set(null);
      },
      error: () => {
        this.guestToRemove.set(null);
      },
    });
  }
}
