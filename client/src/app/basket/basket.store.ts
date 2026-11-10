import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { catchError, of, switchMap, tap } from 'rxjs';

import { BasketApi } from './basket.api';
import { Receipt } from './basket.models';

/**
 * Holds the basket state with signals.
 * Every time the item list changes, the basket is re-priced on the server.
 * switchMap cancels a pricing request that is still running if the user clicks again,
 * so an old, slower response can never overwrite a newer one.
 */
@Injectable({ providedIn: 'root' })
export class BasketStore {
  private readonly api = inject(BasketApi);

  /** The basket as the task describes it: a list of names, e.g. ["Apple", "Apple", "Banana"]. */
  readonly items = signal<string[]>([]);
  readonly receipt = signal<Receipt | null>(null);
  readonly error = signal<string | null>(null);
  readonly loading = signal(false);

  readonly itemCount = computed(() => this.items().length);

  /** How many of each product are in the basket, for the +/- buttons. */
  readonly quantities = computed(() => {
    const counts: Record<string, number> = {};
    for (const name of this.items()) {
      counts[name] = (counts[name] ?? 0) + 1;
    }
    return counts;
  });

  constructor() {
    toObservable(this.items)
      .pipe(
        tap(() => this.loading.set(true)),
        switchMap((items) =>
          this.api.price(items).pipe(
            catchError((err: HttpErrorResponse) => {
              this.error.set(err.error?.error ?? 'Could not price the basket. Is the API running?');
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((receipt) => {
        if (receipt) {
          this.receipt.set(receipt);
          this.error.set(null);
        }
        this.loading.set(false);
      });
  }

  add(name: string): void {
    this.items.update((items) => [...items, name]);
  }

  removeOne(name: string): void {
    this.items.update((items) => {
      const index = items.lastIndexOf(name);
      return index === -1 ? items : [...items.slice(0, index), ...items.slice(index + 1)];
    });
  }

  clear(): void {
    this.items.set([]);
  }
}
