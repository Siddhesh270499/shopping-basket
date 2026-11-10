import { PoundsPipe } from './pounds.pipe';

describe('PoundsPipe', () => {
  const pipe = new PoundsPipe();

  it('formats pence as pounds with two decimals', () => {
    expect(pipe.transform(0)).toBe('£0.00');
    expect(pipe.transform(5)).toBe('£0.05');
    expect(pipe.transform(135)).toBe('£1.35');
    expect(pipe.transform(10005)).toBe('£100.05');
  });

  it('treats a missing value as zero', () => {
    expect(pipe.transform(null)).toBe('£0.00');
    expect(pipe.transform(undefined)).toBe('£0.00');
  });
});
