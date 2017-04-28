import { Pipe, PipeTransform } from '@angular/core';
import { ResourceService } from '../services';
var format = require('string-format')

@Pipe({
    name: 'resx',
    pure: false
})

export class ResourcePipe implements PipeTransform {

    constructor(private resx: ResourceService) { }

    transform(value: string, args: any[]): any {
        if (!value) return;

        if (args) {
            return format(this.resx.Instant(value), args);
        }

        return this.resx.Instant(value);
    }
}