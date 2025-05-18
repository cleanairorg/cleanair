import '../css/MeasurementAlerts.css';

export default function MeasurementAlerts() {
    return (
        <aside className="app measurement-alerts">
            <h2 className="section-title">MEASUREMENTS ALERTS</h2>
            <div className="app alert critical">
                <h3>Critical Alert</h3>
            </div>
            <div className="app alert warning">
                <h3>Warning</h3>
            </div>
        </aside>
    );
}