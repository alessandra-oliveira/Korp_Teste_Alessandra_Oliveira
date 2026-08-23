import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetalheNotaFiscal } from './detalhe-nota-fiscal';

describe('DetalheNotaFiscal', () => {
  let component: DetalheNotaFiscal;
  let fixture: ComponentFixture<DetalheNotaFiscal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetalheNotaFiscal],
    }).compileComponents();

    fixture = TestBed.createComponent(DetalheNotaFiscal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
