import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { map, Observable, switchMap } from 'rxjs';
import { API_BASE_URL } from '@app/shared/config/api-base-url.token';
import { SKIP_ERROR_TOAST } from '@app/auth/skip-error-toast.context';
import { GuestInvite } from '@app/models/reservations/guest-invite.model';
import { GroupSessionsResponse } from '@app/models/group-orders/group-sessions-response.model';
import { GroupStatusResponse } from '@app/models/group-orders/group-status.model';
import { OwnerSessionResponse } from '@app/models/group-orders/owner-session.model';
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

  private buildContext(options?: RequestOptions): HttpContext {
    const context = new HttpContext();
    if (options?.silent) {
      context.set(SKIP_ERROR_TOAST, true);
    }
    return context;
  }

  getGroupInvite$(reservationId: string): Observable<GuestInvite> {
    return this.http
      .get<{ token: string }>(`${this.baseUrl}/reservations/${reservationId}/invite-token`)
      .pipe(switchMap(({ token }) => this.http.get<GuestInvite>(`${this.baseUrl}/guest/invites/${token}`)));
  }

  getSessions$(groupId: string, options?: RequestOptions): Observable<GroupSessionsResponse> {
    return this.http.get<GroupSessionsResponse>(`${this.baseUrl}/guest/groups/${groupId}/sessions`, {
      context: this.buildContext(options),
    });
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
    return this.http.delete<GroupSessionsResponse>(
      `${this.baseUrl}/guest/groups/${groupId}/sessions/${guestSessionId}`
    );
  }
}
