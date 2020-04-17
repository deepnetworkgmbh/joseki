import { Component, Vue, Prop, Watch } from "vue-property-decorator";

@Component
export default class Paginator extends Vue {

    @Prop()
    pageIndex!: number;

    @Prop()
    pageSize!: number;

    @Prop()
    totalRows!: number;

    pageButtons: any[] = [];

    renderButtons() {
        let numberOfPages = Math.ceil(this.totalRows / this.pageSize);
        this.pageButtons = this.pagination(this.pageIndex, numberOfPages);
    }

    /// https://gist.github.com/kottenator/9d936eb3e4e3c3e02598
    pagination(c, m): any[] {
        var current = c,
            last = m,
            delta = 6,
            left = current - delta,
            right = current + delta + 1,
            range: any = [],
            rangeWithDots: any[] = [],
            l;
    
        for (let i = 1; i <= last; i++) {
            if (i == 1 || i == last || i >= left && i < right) {
                range.push(i);
            }
        }

        for (let i of range) {
            if (l) {
                if (i - l === 2) {
                    rangeWithDots.push(l + 1);
                } else if (i - l !== 1) {
                    rangeWithDots.push('...');
                }
            }
            rangeWithDots.push(i);
            l = i;
        }    
        return rangeWithDots;
    }

    getButtonClass(btn) {
        if (btn === '...') {
            return 'pagebutton-delimeter';
        }
        return (btn-1) === this.pageIndex ? 'pagebutton-selected gradient' : 'pagebutton gradient';
    }

    changePageIndex(index) {
        console.log('[index changed]', index);
        if (index === '...') return;
        this.$emit('pageChanged', index);
    }

    @Watch('totalRows', { immediate: true })
    private onTotalRowsChanged(newValue: number) {
        this.renderButtons();
    }

    @Watch('pageIndex', { immediate: true })
    private onPageIndexChanged(newValue: number) {
        this.renderButtons();
    }
}
