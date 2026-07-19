import { Routes } from '@angular/router';
import { bookingSelectionGuard } from './core/guards/booking-selection.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/flight-search/flight-search.page').then((m) => m.FlightSearchPage)
  },
  {
    path: 'booking',
    canActivate: [bookingSelectionGuard],
    loadComponent: () => import('./features/booking/booking.page').then((m) => m.BookingPage)
  },
  {
    path: '**',
    redirectTo: ''
  }
];