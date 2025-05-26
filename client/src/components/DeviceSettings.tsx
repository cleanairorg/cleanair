import '../css/DeviceSettings.css';
import {JwtAtom, UserInfoAtom} from "../atoms.ts";
import {useAtom} from "jotai";
import {cleanAirClient} from "../apiControllerClients.ts";
import toast from "react-hot-toast";

export default function DeviceSettings() {

    const [jwt] = useAtom(JwtAtom);

    const [userInfo,] = useAtom(UserInfoAtom);

    return (
        <section className="app device-settings">
            <h2 className="section-title">DEVICE SETTINGS</h2>
            <div className="app setting">Temperature: </div>
            <div className="app setting">Humidity:  </div>
            <div className="app setting">Air quality: </div>
            <div className="app setting">Air pressure: </div>
            <button className="app interval-button">Interval: </button>
            {userInfo?.role === "admin" && (
            <>
                <button className="app evaluate-button" onClick={() => {
                    cleanAirClient.getMeasurementNow(jwt).then(success => {
                        toast.success("Request sent for new measurements");
                    }).catch(error => {
                        toast.error("Error getting new measurements");
                        console.error(error);
                    });
                }}>New Evaluation Now</button>
                <button className="app delete-button">DELETE DATA</button>
            </>
            )}
        </section>
    );
}