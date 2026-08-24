import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListaNotasFiscaisComponent } from './notas-fiscais';

describe('ListaNotasFiscaisComponent', () => {
  let component: ListaNotasFiscaisComponent;
  let fixture: ComponentFixture<ListaNotasFiscaisComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListaNotasFiscaisComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ListaNotasFiscaisComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
