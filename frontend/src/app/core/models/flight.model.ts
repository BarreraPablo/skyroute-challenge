export interface Flight {
  id: string;
  provider: 'GlobalAir' | 'BudgetWings';
  from: string;
  to: string;
  departureTime: string;
  arrivalTime: string;
  durationMinutes: number;
  price: number;
  cabinClass: 'Economy' | 'Business' | 'First Class';
}