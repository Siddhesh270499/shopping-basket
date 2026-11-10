// Shapes of the JSON returned by the ASP.NET Core API (camelCase, as ASP.NET serialises it).

export interface Product {
  name: string;
  unitPricePence: number;
  offer: string | null;
}

export interface ReceiptLine {
  product: string;
  quantity: number;
  unitPricePence: number;
  costPence: number;
  fullPricePence: number;
  savingPence: number;
  offer: string | null;
}

export interface Receipt {
  lines: ReceiptLine[];
  totalPence: number;
  total: string;
  totalSavingPence: number;
  totalSaving: string;
}
