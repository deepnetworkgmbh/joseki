import axios from 'axios';

export class DataService  {

    public async getOverviewData() {
        console.log(`[] calling kube/overview`);

        return axios.get('/api/kube/overview/')
           .then(function (response) {
            // handle success
            console.log(response);
            return response.data;            
          })
          .catch(function (error) {
            // handle error
            console.log(error);
          })
          .finally(function () {
            // always executed
          });
        //console.log(`[] response`, response);
        
    }
}