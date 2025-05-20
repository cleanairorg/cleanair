import '../css/DeviceSettings.css';
import {JwtAtom, UserInfoAtom} from "../atoms.ts";
import {useAtom} from "jotai";

export default function DeviceSettings() {

    const [jwt] = useAtom(JwtAtom);

    const [userInfo,] = useAtom(UserInfoAtom);

    return (
        <section className="app device-settings">
            <h2 className="section-title">DEVICE SETTINGS</h2>
            <div className="app setting">Temperature: </div>
            <div className="app setting">Humidity:  </div>
            <div className="app setting">CO2: </div>
            <div className="app setting">Air pressure: </div>
            <button className="app interval-button">Interval: </button>
            {userInfo?.role === "admin" && (
            <>
                <button className="app evaluate-button">New Evaluation Now</button>
                <button className="app delete-button">DELETE DATA</button>
            </>
            )}
            <button className="app evaluate-button" onClick={() => {
                weatherStationClient.getMeasurementNow().then(success => {
                    toast.success("New measurements are now available");
                }).catch(error => {
                    toast.error("Error while getting new measurements");
                    console.error(error);
                });
            }}>New Evaluation Now</button>
            <button className="app delete-button">DELETE DATA</button>
        </section>
    );
}