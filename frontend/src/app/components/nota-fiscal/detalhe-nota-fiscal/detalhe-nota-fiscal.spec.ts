import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { DetalheNotaFiscalComponent } from './detalhe-nota-fiscal';

describe('DetalheNotaFiscalComponent', () => {
  let component: DetalheNotaFiscalComponent;
  let fixture: ComponentFixture<DetalheNotaFiscalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetalheNotaFiscalComponent],
      providers: [provideHttpClient(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(DetalheNotaFiscalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
