import '../css/DeviceSettings.css';
import {CurrentValueAtom, DeviceIntervalAtom, JwtAtom, UserInfoAtom, ThresholdsAtom} from "../atoms.ts";
import {useAtom} from "jotai";
import {cleanAirClient, thresholdClient} from "../apiControllerClients.ts";
import toast from "react-hot-toast";
import {useEffect, useState} from "react";
import {useWsClient} from "ws-request-hook";
import {ServerBroadcastsIntervalChange, StringConstants} from "../generated-client.ts";
import ThresholdSlider from "../components/ThresholdSlider";

export default function DeviceSettings() {

    const [jwt] = useAtom(JwtAtom);
    const { onMessage, readyState } = useWsClient();

    const [userInfo,] = useAtom(UserInfoAtom);
    const [currentValue] = useAtom(CurrentValueAtom);
    const [selectedInterval, setSelectedInterval] = useAtom(DeviceIntervalAtom);

    const [currentInterval, setCurrentInterval] = useState<number | null>(null);
    
    const [editing, setEditing] = useState(false);
    const [thresholds, setThresholds] = useAtom(ThresholdsAtom);
    const [getLocalThreshold, setLocalThreshold] = useState(thresholds);
    const [thresholdsExpanded, setThresholdsExpanded] = useState(true);


    const intervals = [
        { min: 1, ms: 60000 },
        { min: 5, ms: 300000 },
        { min: 10, ms: 600000 },
        { min: 15, ms: 900000 },
        { min: 30, ms: 1800000 },
        { min: 45, ms: 2700000 },
        { min: 60, ms: 3600000 }
    ]

    const metricRanges: Record<string, { min: number; max: number }> = {
        temperature: { min: 10, max: 40 },
        humidity: { min: 0, max: 100 },
        airquality: { min: 0, max: 2000 },
        pressure: { min: 990, max: 1030 },
    };

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
    }, [readyState, jwt, onMessage, setSelectedInterval]);

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
    
    
    const updateMetric = (metric: string, values: number[]) => {
        setLocalThreshold(prev => prev.map(t =>
            t.metric === metric ? { ...t, warnMin: values[0], goodMin: values[1], goodMax: values[2], warnMax: values[3] } : t
        ));
    };

    const saveThresholds = () => {
        thresholdClient.updateThresholds({ thresholds: getLocalThreshold }, jwt)
            .then(() => {
                toast.success("Thresholds updated");
                setThresholds(getLocalThreshold);
                setEditing(false);
            })
            .catch(err => {
                toast.error("Error updating thresholds");
                console.error(err);
            });
    };
    
    
    return (
        <section className="app device-settings">
            <h2 className="section-title">DEVICE SETTINGS</h2>

            {/* Kollapsible Thresholds Section */}
            <div className="thresholds-section">
                <div
                    className="thresholds-header"
                    onClick={() => setThresholdsExpanded(!thresholdsExpanded)}
                >
                    <span className="thresholds-title">SENSOR THRESHOLDS</span>
                    <span className={`expand-arrow ${thresholdsExpanded ? 'expanded' : ''}`}>
                        ▼
                    </span>
                </div>

                {/* HELE indholdet kollapser sammen - både sliders og edit knapper */}
                {thresholdsExpanded && (
                    <div className="thresholds-wrapper">
                        {getLocalThreshold.map(t => (
                            <ThresholdSlider
                                key={t.metric}
                                metric={t.metric}
                                values={[t.warnMin, t.goodMin, t.goodMax, t.warnMax]}
                                onChange={vals => updateMetric(t.metric, vals)}
                                disabled={!editing}
                                min={metricRanges[t.metric]?.min || 0}
                                max={metricRanges[t.metric]?.max || 100}
                            />
                        ))}

                        {/* Edit/Save buttons */}
                        {userInfo?.role === "admin" && (
                            <>
                                {!editing && (
                                    <button
                                        className="app interval-button"
                                        onClick={() => setEditing(true)}
                                        style={{marginTop: '1rem'}}
                                    >
                                        Edit Thresholds
                                    </button>
                                )}

                                {editing && (
                                    <div style={{display: 'flex', gap: '10px', marginTop: '1rem'}}>
                                        <button className="app interval-button" onClick={saveThresholds}>
                                            Save Changes
                                        </button>
                                        <button
                                            className="delete-button"
                                            onClick={() => {
                                                setLocalThreshold(thresholds);
                                                setEditing(false);
                                            }}
                                        >
                                            Cancel
                                        </button>
                                    </div>
                                )}
                            </>
                        )}
                    </div>
                )}
            </div>

                <div className="app setting">Current Interval: {displayCurrentInterval(currentInterval)}</div>
                
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
                <button className="app delete-button" onClick={handleDeleteData}>DELETE DATA</button>
        </section>
    );
}