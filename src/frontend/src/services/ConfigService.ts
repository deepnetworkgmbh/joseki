
export class ConfigService {

    public static Timezone: string;
    public static ApiUrl: string;

    public static async Init() {
        const runtimeConfig = await fetch('/config.json');
        const json = await runtimeConfig.json();
        ConfigService.Timezone = json.TIMEZONE;
        ConfigService.ApiUrl = json.API_URL;
        console.log(`[ApiUrl=${ConfigService.ApiUrl}]`);
        console.log(`[Timezone=${ConfigService.Timezone}]`);
    }

}