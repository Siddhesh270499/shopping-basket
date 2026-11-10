import { Pipe, PipeTransform } from '@angular/core';

/** 135 -> "£1.35". The API sends whole pence, so the UI only formats. */
@Pipe({ name: 'pounds' })
export class PoundsPipe implements PipeTransform {
  transform(pence: number | null | undefined): string {
    return '£' + ((pence ?? 0) / 100).toFixed(2);
  }
}
