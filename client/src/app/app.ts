import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';

import { BasketApi } from './basket/basket.api';
import { BasketStore } from './basket/basket.store';
import { ProductList } from './product-list/product-list';
import { ReceiptSummary } from './receipt-summary/receipt-summary';

/** Page layout: products on the left, the priced basket on the right. */
@Component({
  selector: 'app-root',
  imports: [ProductList, ReceiptSummary],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly store = inject(BasketStore);
  protected readonly products = toSignal(inject(BasketApi).getProducts(), { initialValue: [] });
}
