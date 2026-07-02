import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Productsearch } from './productsearch';

describe('Productsearch', () => {
  let component: Productsearch;
  let fixture: ComponentFixture<Productsearch>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Productsearch],
    }).compileComponents();

    fixture = TestBed.createComponent(Productsearch);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
