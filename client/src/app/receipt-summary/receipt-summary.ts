import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { Receipt } from '../basket/basket.models';
import { PoundsPipe } from '../shared/pounds.pipe';

/** The priced basket: one row per product, savings from offers, and the total. */
@Component({
  selector: 'app-receipt-summary',
  imports: [PoundsPipe],
  templateUrl: './receipt-summary.html',
  styleUrl: './receipt-summary.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReceiptSummary {
  readonly receipt = input<Receipt | null>(null);
  readonly loading = input(false);
}
