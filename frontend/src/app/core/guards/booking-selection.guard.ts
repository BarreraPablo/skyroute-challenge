import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { BookingSelectionService } from '../services/booking-selection.service';

export const bookingSelectionGuard: CanActivateFn = () => {
  const bookingSelectionService = inject(BookingSelectionService);
  const router = inject(Router);

  return bookingSelectionService.selection() ? true : router.parseUrl('/');
};