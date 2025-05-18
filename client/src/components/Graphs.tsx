import '../css/Graphs.css';

export default function Graphs() {
    return (
        <div className="graphs-container">
            <div className="graph-card">
                <div className="room-name">Front room</div>
                <div className="temperature">19.5<span className="temperature-unit">°C</span></div>
                <div className="graph-line temperature-line"></div>
            </div>
            <div className="graph-card">
                <div className="humidity">53.8<span className="humidity-unit">%</span></div>
                <div className="graph-line humidity-line"></div>
            </div>
            <div className="graph-card">
                <div className="humidity">53.8<span className="humidity-unit">%</span></div>
                <div className="graph-line humidity-line"></div>
            </div>
            <div className="graph-card">
                <div className="humidity">53.8<span className="humidity-unit">%</span></div>
                <div className="graph-line humidity-line"></div>
            </div>
        </div>
    );
}