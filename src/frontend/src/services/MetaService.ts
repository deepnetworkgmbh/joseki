import { MetaData } from '@/models';
import { DataService } from './DataService';
import { DateTime } from 'luxon';

/**
 * MetaService is a key-data store returning metadata loaded from api
 * It initializes on app startup and serves data from memory
 * 
 * @export
 * @class MetaService
 */
export class MetaService {

    private static model?: MetaDataModel

    /**
     * Load and store metadata
     *
     * @static
     * @memberof MetaService
     */
    public static async Init() {        
        let data = localStorage.getItem('metadata');
        if(data !== null) {
            //console.log('local meta data found, checking.')
            let model = <MetaDataModel>JSON.parse(data);
            //console.log(`data date day is ${model.date}`);
            //console.log(`today date day is ${DateTime.utc()}`);
            if(model.date.toString().split('T')[0] === DateTime.utc().toString().split('T')[0]) {
                //console.log('same day data found, exiting.')
                MetaService.model = model;
                return;
            }
        } 
        console.log('Initializing meta service...');
        let service = new DataService();
        await service.getWebsiteMeta()
               .then(data => {
                   if(data) {
                       let model = new MetaDataModel();
                       model.date = DateTime.utc();
                       model.data = data;
                       MetaService.model = model;
                       localStorage.setItem('metadata', JSON.stringify(model));
                       //console.log('new meta stored.');
                    }
                });                
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
        if(MetaService.model === undefined) return '';
        let rowIndex = MetaService.model!.data.findIndex(x => x.id === key);
        if (rowIndex === -1) {
            // if a key was not found, invalidate cache
            localStorage.removeItem('metadata');
            return '';
        }
        return MetaService.model!.data[rowIndex].content;
    }

}

export class MetaDataModel {
    date!: DateTime;
    data: MetaData[] = []
}