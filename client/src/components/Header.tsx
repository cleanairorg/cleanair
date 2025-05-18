import '../css/Header.css';

export default function Header() {
    return (
        <header className="app header">
            <div className="header-left">
                <button className="app login-button">LOGIN</button>
                <button className="app register-button">REGISTER</button>
            </div>
            <div className="header-center">
                <div className="app selected-device">SELECTED DEVICE ▼</div>
            </div>
            <div className="header-right">
                <div className="app logo">CleanAir</div>
            </div>
        </header>
    );
}
