import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CreateBookingPassengerRequest {
  fullName: string;
  email: string;
  documentNumber: string;
}

export interface CreateBookingRequest {
  flightId: string;
  provider: string;
  originCode: string;
  destinationCode: string;
  departureTimeUtc: string;
  arrivalTimeUtc: string;
  cabinClass: string;
  pricePerPassenger: number;
  passengerCount: number;
  expectedPrice: number;
  passenger: CreateBookingPassengerRequest;
}

export interface CreateBookingResponse {
  bookingReference: string;
}

export interface BookingValidationCondition {
  severity: 'Error' | 'Warning';
  message: string;
}

export interface BookingValidationResult {
  statusCode: number;
  conditions: BookingValidationCondition[];
}

@Injectable({ providedIn: 'root' })
export class BookingApiService {
  private readonly http = inject(HttpClient);

  create(request: CreateBookingRequest): Observable<CreateBookingResponse> {
    return this.http.post<CreateBookingResponse>(`${environment.apiBaseUrl}/bookings`, request);
  }
}