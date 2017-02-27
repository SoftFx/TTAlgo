import { Pipe, PipeTransform } from '@angular/core';
import { ResourceService } from '../services';

@Pipe({
    name: 'resx',
    pure: false
})

export class ResourcePipe implements PipeTransform {

	constructor(private resx: ResourceService) { }

	transform(value: string, args: any[]): any {
		if (!value) return;
		
		return this.resx.instant(value);
	}
}