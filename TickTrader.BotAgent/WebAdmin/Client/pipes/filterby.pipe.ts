﻿import { Pipe, Injectable } from '@angular/core';

@Pipe({
    name: 'filterBy',
    pure: false
})

export class FilterByPipe {

    private filterByString(filter) {
        filter = filter.toLowerCase();
        return value => {
            return !filter || value.toLowerCase().indexOf(filter) !== -1;
        }
    }

    private filterByBoolean(filter) {
        return value => {
            return Boolean(value) === filter;
        }
    }

    private filterByObject(filter) {
        return value => {
            for (let key in filter) {
                if (!value.hasOwnProperty(key)) {
                    return false;
                }

                const type = typeof filter[key];
                let isMatching;

                if (type === 'boolean') {
                    isMatching = this.filterByBoolean(filter[key])(value[key]);
                } else if (type === 'string') {
                    isMatching = this.filterByString(filter[key])(value[key]);
                } else if (type === 'object') {
                    isMatching = this.filterByObject(filter[key])(value[key]);
                } else {
                    isMatching = this.filterDefault(filter[key])(value[key]);
                }

                if (!isMatching) {
                    return false;
                }
            }

            return true;
        }
    }

    private filterDefault(filter) {
        return value => {
            return !filter || filter == value;
        }
    }

    private isNumber(value) {
        return !isNaN(parseInt(value, 10)) && isFinite(value);
    }

    transform(array: any[], filter: any): any {
        const type = typeof filter;

        if (!array) {
            return array;
        }

        if (type === 'boolean') {
            return array.filter(this.filterByBoolean(filter));
        }

        if (type === 'string') {
            if (this.isNumber(filter)) {
                return array.filter(this.filterDefault(filter));
            }

            return array.filter(this.filterByString(filter));
        }

        if (type === 'object') {
            return array.filter(this.filterByObject(filter));
        }

        return array.filter(this.filterDefault(filter));
    }
}