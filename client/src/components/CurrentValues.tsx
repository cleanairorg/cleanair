import '../css/CurrentValues.css';

export default function CurrentValues() {
    return (
        <section className="current-values">
            <h2 className="section-title">CURRENT VALUES</h2>
            <div className="value yellow">Temperature: °</div>
            <div className="value green">Humidity: </div>
            <div className="value red">CO2: </div>
            <div className="value green">Air pressure: </div>
            <div className="last-week">Last Week</div>
        </section>
    );
}
