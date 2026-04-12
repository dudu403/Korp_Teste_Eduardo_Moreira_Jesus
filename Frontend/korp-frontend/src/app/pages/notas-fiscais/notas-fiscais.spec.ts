import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotasFiscais } from './notas-fiscais';

describe('NotasFiscais', () => {
  let component: NotasFiscais;
  let fixture: ComponentFixture<NotasFiscais>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotasFiscais]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotasFiscais);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
