
export class ConfigService {

    public static Timezone: string;
    public static ApiUrl: string;
    public static ClientID: string;
    public static TenantID: string;
    public static Domain: string;
    public static AuthEnabled: boolean;

    public static async Init() {
        const runtimeConfig = await fetch('/config.json');
        const json = await runtimeConfig.json();
        ConfigService.Timezone = json.TIMEZONE;
        ConfigService.ApiUrl = json.API_URL;
        console.log(`[ApiUrl=${ConfigService.ApiUrl}]`);
        console.log(`[Timezone=${ConfigService.Timezone}]`);

        ConfigService.AuthEnabled = json.AUTHENABLED;
        console.log(`[Timezone=${ConfigService.Timezone}]`);
        if (ConfigService.AuthEnabled) {
            ConfigService.ClientID = json.CLIENT_ID;
            ConfigService.TenantID = json.TENANT_ID;
            ConfigService.Domain = json.DOMAIN;
            if (ConfigService.ClientID && ConfigService.TenantID && ConfigService.Domain) {
                console.log(`[Authentication enbled]`);
            }else {
                console.log(`[Authentication configuration missing]`);
            }
        }else {
            console.log(`[Authentication disabled]`);
        }
    }
}
