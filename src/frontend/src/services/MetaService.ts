import { MetaData } from '@/models';
import { DataService } from './DataService';

export class MetaService {

    private static data: MetaData[] = []

    public static async Init() {        
        let service = new DataService();
        await service.getWebsiteMeta()
               .then(data => {
                   if(data) {
                        MetaService.data = data
                        console.log(`[meta]`, data);
                   }
                })
               .then(()=> console.log(MetaService.data))
               .catch((error)=> console.log(`[] failed fetching meta: ${error}`));
    }

    public static Get(key: string): string {
        let rowIndex = MetaService.data.findIndex(x => x.id === key);
        if (rowIndex === -1) return '';
        return MetaService.data[rowIndex].content;
    }

}