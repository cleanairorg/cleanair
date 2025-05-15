import '../css/DeviceSettings.css';

export default function DeviceSettings() {
    return (
        <section className="app device-settings">
            <h2 className="section-title">DEVICE SETTINGS</h2>
            <div className="app setting">Temperature: </div>
            <div className="app setting">Humidity:  </div>
            <div className="app setting">CO2: </div>
            <div className="app setting">Air pressure: </div>
            <button className="app interval-button">Interval: </button>
            <button className="app evaluate-button">New Evaluation Now</button>
            <button className="app delete-button">DELETE DATA</button>
        </section>
    );
}