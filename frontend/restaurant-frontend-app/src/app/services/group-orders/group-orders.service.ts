import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpHeaders } from '@angular/common/http';
import { map, Observable, switchMap } from 'rxjs';
import { API_BASE_URL } from '@app/shared/config/api-base-url.token';
import { SKIP_ERROR_TOAST } from '@app/auth/skip-error-toast.context';
import { AuthenticationService } from '@app/auth/services/authentication.service';
import { GuestInvite } from '@app/models/reservations/guest-invite.model';
import { GroupSessionsResponse } from '@app/models/group-orders/group-sessions-response.model';
import { GroupStatusResponse } from '@app/models/group-orders/group-status.model';
import { OwnerSessionResponse } from '@app/models/group-orders/owner-session.model';
import { Participant } from '@app/models/group-orders/participant.model';
import { SubmittedDish } from '@app/models/group-orders/submit-group-pre-order.model';

export interface RequestOptions {
  silent?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class GroupOrdersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly authService = inject(AuthenticationService);

  private buildContext(options?: RequestOptions): HttpContext {
    const context = new HttpContext();
    if (options?.silent) {
      context.set(SKIP_ERROR_TOAST, true);
    }
    return context;
  }

  private getAuthHeaders(): HttpHeaders {
    const tokens = this.authService.getTokens();
    if (tokens?.idToken) {
      return new HttpHeaders({ Authorization: `Bearer ${tokens.idToken}` });
    }
    return new HttpHeaders();
  }

  private normalizeSessionsResponse(raw: unknown): GroupSessionsResponse {
    if (!raw || typeof raw !== 'object') {
      return { participants: [], total: 0 };
    }
    const r = raw as Record<string, unknown>;
    const rawParticipants = (r['participants'] ?? r['Participants'] ?? []) as Record<string, unknown>[];
    const participants: Participant[] = rawParticipants.map((p) => {
      const rawItems = (p['items'] ?? p['Items'] ?? []) as Record<string, unknown>[];
      return {
        id: (p['id'] ?? p['Id'] ?? '') as string,
        type: ((p['type'] ?? p['Type'] ?? 'Guest') as string) === 'Owner' ? 'Owner' : 'Guest',
        name: (p['name'] ?? p['Name'] ?? '') as string,
        items: rawItems.map((item) => ({
          dish: (item['dish'] ?? item['Dish'] ?? {}) as Participant['items'][0]['dish'],
          quantity: Number(item['quantity'] ?? item['Quantity'] ?? 0),
        })),
        total: Number(p['total'] ?? p['Total'] ?? 0),
        isSubmitted: Boolean(p['isSubmitted'] ?? p['IsSubmitted'] ?? false),
      };
    });

    return {
      participants,
      total: Number(r['total'] ?? r['Total'] ?? 0),
    };
  }

  getGroupInvite$(reservationId: string): Observable<GuestInvite> {
    return this.http
      .get<{ token: string }>(`${this.baseUrl}/reservations/${reservationId}/invite-token`)
      .pipe(switchMap(({ token }) => this.http.get<GuestInvite>(`${this.baseUrl}/guest/invites/${token}`)));
  }

  getSessions$(groupId: string, options?: RequestOptions): Observable<GroupSessionsResponse> {
    return this.http
      .get<unknown>(`${this.baseUrl}/guest/groups/${groupId}/sessions`, {
        headers: this.getAuthHeaders(),
        context: this.buildContext(options),
      })
      .pipe(map((res) => this.normalizeSessionsResponse(res)));
  }

  getOwnerSessionId$(reservationId: string): Observable<string> {
    return this.http
      .get<OwnerSessionResponse>(`${this.baseUrl}/reservations/${reservationId}/guest-session-id`)
      .pipe(map((res) => res.guestSessionId));
  }

  getStatus$(groupId: string, options?: RequestOptions): Observable<GroupStatusResponse> {
    return this.http.get<GroupStatusResponse>(`${this.baseUrl}/guest/groups/${groupId}/status`, {
      context: this.buildContext(options),
    });
  }

  submit$(groupId: string, reservationId: string): Observable<SubmittedDish[]> {
    return this.http.post<SubmittedDish[]>(`${this.baseUrl}/group-reservations/${groupId}/submit`, {
      reservationId,
    });
  }

  removeSession$(groupId: string, guestSessionId: string): Observable<GroupSessionsResponse> {
    return this.http
      .delete<unknown>(
        `${this.baseUrl}/guest/groups/${groupId}/sessions/${guestSessionId}`,
        { headers: this.getAuthHeaders() }
      )
      .pipe(map((res) => this.normalizeSessionsResponse(res)));
  }
}
