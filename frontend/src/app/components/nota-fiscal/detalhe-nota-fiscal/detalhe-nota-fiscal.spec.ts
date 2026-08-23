import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetalheNotaFiscalComponent } from './detalhe-nota-fiscal';

describe('DetalheNotaFiscalComponent', () => {
  let component: DetalheNotaFiscalComponent;
  let fixture: ComponentFixture<DetalheNotaFiscalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetalheNotaFiscalComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DetalheNotaFiscalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
