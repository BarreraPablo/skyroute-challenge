import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/flight-search/flight-search.page').then((m) => m.FlightSearchPage)
  },
  {
    path: 'booking',
    loadComponent: () => import('./features/booking/booking.page').then((m) => m.BookingPage)
  },
  {
    path: '**',
    redirectTo: ''
  }
];