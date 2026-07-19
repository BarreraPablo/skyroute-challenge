import { Injectable, signal } from '@angular/core';
import { BookingSelection } from '../models/booking-selection.model';

@Injectable({ providedIn: 'root' })
export class BookingSelectionService {
  private readonly storageKey = 'skyroute.bookingSelection';
  private readonly selectedFlight = signal<BookingSelection | null>(this.readSelection());

  readonly selection = this.selectedFlight.asReadonly();

  setSelection(selection: BookingSelection): void {
    this.selectedFlight.set(selection);
    this.writeSelection(selection);
  }

  clearSelection(): void {
    this.selectedFlight.set(null);
    this.clearStoredSelection();
  }

  private readSelection(): BookingSelection | null {
    if (typeof window === 'undefined') {
      return null;
    }

    const storedSelection = window.sessionStorage.getItem(this.storageKey);

    if (!storedSelection) {
      return null;
    }

    try {
      return JSON.parse(storedSelection) as BookingSelection;
    } catch {
      this.clearStoredSelection();
      return null;
    }
  }

  private writeSelection(selection: BookingSelection): void {
    if (typeof window === 'undefined') {
      return;
    }

    window.sessionStorage.setItem(this.storageKey, JSON.stringify(selection));
  }

  private clearStoredSelection(): void {
    if (typeof window === 'undefined') {
      return;
    }

    window.sessionStorage.removeItem(this.storageKey);
  }
}