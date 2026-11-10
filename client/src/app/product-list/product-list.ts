import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { Product } from '../basket/basket.models';
import { PoundsPipe } from '../shared/pounds.pipe';

/** Shows one card per product with its price, offer and +/- buttons. Presentational only: it emits events. */
@Component({
  selector: 'app-product-list',
  imports: [PoundsPipe],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductList {
  readonly products = input.required<Product[]>();
  readonly quantities = input<Record<string, number>>({});

  readonly add = output<string>();
  readonly remove = output<string>();
}
