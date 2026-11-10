import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { App } from './app';
import { Product } from './basket/basket.models';

const products: Product[] = [
  { name: 'Apple', unitPricePence: 35, offer: null },
  { name: 'Melon', unitPricePence: 50, offer: 'Buy one get one free' },
];

describe('App', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lists the products and shows the priced basket after adding one', async () => {
    const fixture = TestBed.createComponent(App);
    http.expectOne('/api/products').flush(products);
    TestBed.tick();
    http.expectOne('/api/basket/price').flush({ lines: [], totalPence: 0, total: '£0.00', totalSavingPence: 0, totalSaving: '£0.00' });
    await fixture.whenStable();

    const page = fixture.nativeElement as HTMLElement;
    expect(page.querySelectorAll('app-product-list article').length).toBe(2);
    expect(page.textContent).toContain('Buy one get one free');

    (page.querySelector('[aria-label="Add one Melon"]') as HTMLButtonElement).click();
    TestBed.tick();

    const req = http.expectOne('/api/basket/price');
    expect(req.request.body).toEqual(['Melon']);
    req.flush({
      lines: [{ product: 'Melon', quantity: 1, unitPricePence: 50, costPence: 50, fullPricePence: 50, savingPence: 0, offer: 'Buy one get one free' }],
      totalPence: 50,
      total: '£0.50',
      totalSavingPence: 0,
      totalSaving: '£0.00',
    });
    await fixture.whenStable();

    expect(page.querySelector('[data-testid="total"]')?.textContent).toBe('£0.50');
  });
});
