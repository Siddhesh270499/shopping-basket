import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { Receipt } from './basket.models';
import { BasketStore } from './basket.store';

const receipt = (totalPence: number): Receipt => ({
  lines: [],
  totalPence,
  total: '£' + (totalPence / 100).toFixed(2),
  totalSavingPence: 0,
  totalSaving: '£0.00',
});

describe('BasketStore', () => {
  let store: BasketStore;
  let http: HttpTestingController;

  /** Lets the signal -> observable bridge run, then returns the pricing request it sent. */
  const nextPriceRequest = () => {
    TestBed.tick();
    return http.expectOne({ method: 'POST', url: '/api/basket/price' });
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    store = TestBed.inject(BasketStore);
    http = TestBed.inject(HttpTestingController);

    nextPriceRequest().flush(receipt(0)); // the empty basket is priced on start
  });

  afterEach(() => http.verify());

  it('sends the basket as a list of names and stores the receipt', () => {
    store.add('Apple');
    store.add('Apple');
    store.add('Banana');

    const req = nextPriceRequest();
    expect(req.request.body).toEqual(['Apple', 'Apple', 'Banana']);
    req.flush(receipt(90));

    expect(store.receipt()?.total).toBe('£0.90');
    expect(store.loading()).toBe(false);
  });

  it('counts quantities per product', () => {
    store.add('Melon');
    store.add('Lime');
    store.add('Melon');
    nextPriceRequest().flush(receipt(65));

    expect(store.quantities()).toEqual({ Melon: 2, Lime: 1 });
    expect(store.itemCount()).toBe(3);
  });

  it('removes one item at a time and clears the basket', () => {
    store.add('Lime');
    store.add('Lime');
    store.removeOne('Lime');
    nextPriceRequest().flush(receipt(15));
    expect(store.items()).toEqual(['Lime']);

    store.removeOne('Apple'); // not in the basket: nothing changes
    expect(store.items()).toEqual(['Lime']);

    store.clear();
    nextPriceRequest().flush(receipt(0));
    expect(store.items()).toEqual([]);
  });

  it('shows the API error message when an item is rejected', () => {
    store.add('Mango');

    nextPriceRequest().flush({ error: "'Mango' is not a known item." }, { status: 400, statusText: 'Bad Request' });

    expect(store.error()).toBe("'Mango' is not a known item.");
    expect(store.loading()).toBe(false);
  });
});
