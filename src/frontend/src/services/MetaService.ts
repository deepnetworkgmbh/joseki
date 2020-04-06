import { MetaData } from '@/models';
import { DataService } from './DataService';

/**
 * MetaService is a key-data store returning metadata loaded from api
 * It initializes on app startup and serves data from memory
 * 
 * @export
 * @class MetaService
 */
export class MetaService {

    private static data: MetaData[] = []

    /**
     * Load and store metadata
     *
     * @static
     * @memberof MetaService
     */
    public static async Init() {        
        let service = new DataService();
        await service.getWebsiteMeta()
               .then(data => {
                   if(data) {
                        MetaService.data = data
                   }
                })
               .then(()=> console.log(MetaService.data))
               .catch((error)=> console.log(`[] failed fetching meta: ${error}`));
    }
    
    /**
     * return metadata using a key
     *
     * @static
     * @param {string} key
     * @returns {string}
     * @memberof MetaService
     */
    public static Get(key: string): string {
        let rowIndex = MetaService.data.findIndex(x => x.id === key);
        if (rowIndex === -1) return '';
        return MetaService.data[rowIndex].content;
    }

}