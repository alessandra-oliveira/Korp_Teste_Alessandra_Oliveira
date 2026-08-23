import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NovaNotaFiscalComponent } from './nova-nota-fiscal';

describe('NovaNotaFiscalComponent', () => {
  let component: NovaNotaFiscalComponent;
  let fixture: ComponentFixture<NovaNotaFiscalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NovaNotaFiscalComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(NovaNotaFiscalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
