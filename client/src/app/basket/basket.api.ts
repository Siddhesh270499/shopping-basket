import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { Product, Receipt } from './basket.models';

/** Talks to the backend. All prices are worked out on the server; the UI never does the maths. */
@Injectable({ providedIn: 'root' })
export class BasketApi {
  private readonly http = inject(HttpClient);

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>('/api/products');
  }

  price(items: string[]): Observable<Receipt> {
    return this.http.post<Receipt>('/api/basket/price', items);
  }
}
