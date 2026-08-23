import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NovaNotaFiscal } from './nova-nota-fiscal';

describe('NovaNotaFiscal', () => {
  let component: NovaNotaFiscal;
  let fixture: ComponentFixture<NovaNotaFiscal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NovaNotaFiscal],
    }).compileComponents();

    fixture = TestBed.createComponent(NovaNotaFiscal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
