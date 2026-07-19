import { Injectable } from '@angular/core';
import { Booking } from '../models/booking.model';

@Injectable({ providedIn: 'root' })
export class BookingApiService {
  create(booking: Booking): void {
    void booking;
  }
}