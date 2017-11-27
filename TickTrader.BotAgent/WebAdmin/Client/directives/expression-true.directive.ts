import { Directive, Input } from '@angular/core';
import { NG_VALIDATORS, Validator, AbstractControl } from '@angular/forms';
 
@Directive({ 
  selector: '[expressionTrue]',
  providers: [{provide: NG_VALIDATORS, useExisting: ExpressionTrue, multi: true}] 
})
export class ExpressionTrue implements Validator{
  @Input('expressionTrue') predicate: () => boolean;
 
  validate(control: AbstractControl) {

    console.log('ExpressionTrue!!');
    if(this.predicate != null && !this.predicate()){
        return {'expressionTrue': false};
    }
    return null;
  }
}