import '../css/DeviceSettings.css';
import {CurrentValueAtom, DeviceIntervalAtom, JwtAtom, UserInfoAtom} from "../atoms.ts";
import {useAtom} from "jotai";
import {cleanAirClient} from "../apiControllerClients.ts";
import toast from "react-hot-toast";
import {useEffect, useState} from "react";
import {useWsClient} from "ws-request-hook";
import {ServerBroadcastsIntervalChange, StringConstants} from "../generated-client.ts";

export default function DeviceSettings() {

    const [jwt] = useAtom(JwtAtom);
    const { onMessage, readyState } = useWsClient();

    const [userInfo,] = useAtom(UserInfoAtom);
    const [currentValue] = useAtom(CurrentValueAtom);
    const [selectedInterval, setSelectedInterval] = useAtom(DeviceIntervalAtom);

    const [currentInterval, setCurrentInterval] = useState<number | null>(null);


    const intervals = [
        { min: 1, ms: 60000 },
        { min: 5, ms: 300000 },
        { min: 10, ms: 600000 },
        { min: 15, ms: 900000 },
        { min: 30, ms: 1800000 },
        { min: 45, ms: 2700000 },
        { min: 60, ms: 3600000 }
    ]

    useEffect(() => {
        if (readyState != 1 || jwt == null || jwt.length < 1) {
            return;
        }
        if (currentValue && currentValue.interval) {
            setCurrentInterval(currentValue.interval);
        }
    }, [readyState, currentValue, jwt]);

    useEffect(() => {
        if (readyState !== 1 || !jwt || jwt.length < 1) {
            return;
        }

        return onMessage<ServerBroadcastsIntervalChange>(
            StringConstants.ServerBroadcastsIntervalChange, (dto) => {
                if (dto && dto.interval) {
                    const intervalMin = intervals.find(i =>
                        i.ms === dto.interval)?.min || dto.interval / 60000;
                    toast.success("Interval changed to " + intervalMin + " minute(s)");
                    setSelectedInterval(dto);
                }
            }
        );
    }, [readyState, jwt, intervals, onMessage, setSelectedInterval]);

    const handleIntervalChange = () => {
        if (!selectedInterval){
            return;
        }
        cleanAirClient.adminChangesDeviceInterval(jwt, selectedInterval).then(() => {
            setCurrentInterval(selectedInterval.interval!);
        }).catch(err => {
            toast.error("Error changing interval");
        });
    };

    const displayCurrentInterval = (ms: number | null) => {
        if (ms === null){
            return "";
        }
        const min = ms / 60000;
        return `${min} minute(s)`;
    }

    const handleDeleteData = () => {
        if (!window.confirm("Are you sure you want to delete all data? This cannot be undone.")) {
            return;
        }
        cleanAirClient.deleteData(jwt).then(() => {
            toast.success("All data deleted successfully");
        }).catch(err => {
            toast.error("Error deleting data");
            console.error(err);
        });
    };
    
    
    return (
        <section className="app device-settings">
            <h2 className="section-title">DEVICE SETTINGS</h2>
            <div className="app setting">Temperature: </div>
            <div className="app setting">Humidity:  </div>
            <div className="app setting">Air quality: </div>
            <div className="app setting">Air pressure: </div>
            <div className="app setting">Current Interval: {displayCurrentInterval(currentInterval)}</div>
            {userInfo?.role === "admin" && (
            <>
                <div className="interval-container">
                    <fieldset className="fieldset">
                        <select defaultValue="" className="select setting" onChange={e => setSelectedInterval({
                            interval: parseInt(e.target.value)
                        })}>
                            <option disabled>Default Interval 1 min</option>
                            { intervals.map(i => (
                                <option key={i.ms} value={i.ms}>
                                    {i.min} minute(s)
                                </option>
                            ))}
                        </select>
                    </fieldset>
                    <button className="app interval-button" onClick={handleIntervalChange} disabled={!selectedInterval}>
                        Change Interval
                    </button>
                </div>
                <button className="app evaluate-button" onClick={() => {
                    cleanAirClient.getMeasurementNow(jwt).then(success => {
                        toast.success("Request sent for new measurements");
                    }).catch(error => {
                        toast.error("Error getting new measurements");
                        console.error(error);
                    });
                }}>
                    New Evaluation Now
                </button>
                <button className="app delete-button" onClick={handleDeleteData}>DELETE DATA</button>
            </>
            )}
        </section>
    );
}