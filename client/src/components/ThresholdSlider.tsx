import React, { useState, useRef, useCallback } from 'react';
import '../css/DeviceSettings.css';

interface ThresholdSliderProps {
    metric: string;
    values: number[];
    onChange: (newValues: number[]) => void;
    disabled: boolean;
    min: number;
    max: number;
}

export default function ThresholdSlider(
    {
        metric = "temperature",
        values = [0, 25, 75, 100],
        onChange = () => {},
        disabled = false,
        min = 0,
        max = 100
    }: ThresholdSliderProps) {
    
    const [activeThumb, setActiveThumb] = useState<number | null>(null);
    const sliderRef = useRef<HTMLDivElement>(null);

    // Ensure values array has 4 elements
    const safeValues = values.length === 4 ? values : [min, min + (max-min)*0.25, min + (max-min)*0.75, max];
    const [warnMin, goodMin, goodMax, warnMax] = safeValues;

    // Calculate percentages for positioning
    const range = max - min;
    const warnMinPercent = ((warnMin - min) / range) * 100;
    const goodMinPercent = ((goodMin - min) / range) * 100;
    const goodMaxPercent = ((goodMax - min) / range) * 100;
    const warnMaxPercent = ((warnMax - min) / range) * 100;

    const handleMouseDown = useCallback((thumbIndex: number, event: React.MouseEvent) => {
        if (disabled) return;

        event.preventDefault();
        setActiveThumb(thumbIndex);

        const handleMouseMove = (e: MouseEvent) => {
            if (!sliderRef.current) return;

            const rect = sliderRef.current.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const percentage = Math.max(0, Math.min(100, (x / rect.width) * 100));
            const newValue = Math.round(min + (percentage / 100) * range);

            const newValues = [...safeValues];
            newValues[thumbIndex] = newValue;

            // Enforce constraints
            if (thumbIndex === 0 && newValues[0] >= newValues[1]) {
                newValues[0] = Math.max(min, newValues[1] - 1);
            }
            if (thumbIndex === 1 && (newValues[1] >= newValues[2] || newValues[1] <= newValues[0])) {
                newValues[1] = Math.max(newValues[0] + 1, Math.min(newValues[2] - 1, newValue));
            }
            if (thumbIndex === 2 && (newValues[2] <= newValues[1] || newValues[2] >= newValues[3])) {
                newValues[2] = Math.min(newValues[3] - 1, Math.max(newValues[1] + 1, newValue));
            }
            if (thumbIndex === 3 && newValues[3] <= newValues[2]) {
                newValues[3] = Math.min(max, newValues[2] + 1);
            }

            onChange(newValues);
        };

        const handleMouseUp = () => {
            setActiveThumb(null);
            document.removeEventListener('mousemove', handleMouseMove);
            document.removeEventListener('mouseup', handleMouseUp);
        };

        document.addEventListener('mousemove', handleMouseMove);
        document.addEventListener('mouseup', handleMouseUp);
    }, [safeValues, onChange, disabled, min, max, range]);

    const thumbStyle = (percent: number, index: number) => ({
        position: 'absolute' as const,
        left: `${percent}%`,
        top: '50%',
        transform: 'translate(-50%, -50%)',
        width: '16px',
        height: '16px',
        borderRadius: '50%',
        background: 'white',
        border: '2px solid #888',
        cursor: disabled ? 'default' : 'pointer',
        zIndex: activeThumb === index ? 10 : 5,
        boxShadow: activeThumb === index ? '0 0 0 3px rgba(66, 153, 225, 0.5)' : 'none'
    });

    return (
        <div className="threshold-slider">
            <div className="metric-label">{metric.toUpperCase()}</div>

            {/* Main slider track */}
            <div
                ref={sliderRef}
                style={{
                    position: 'relative',
                    height: '40px',
                    width: '100%',
                    display: 'flex',
                    alignItems: 'center',
                    cursor: disabled ? 'default' : 'pointer'
                }}
            >
                {/* Background track with gradient */}
                <div style={{
                    position: 'absolute',
                    top: '50%',
                    left: 0,
                    right: 0,
                    height: '6px',
                    transform: 'translateY(-50%)',
                    borderRadius: '3px',
                    background: `linear-gradient(to right,
                        #ff4444 0%,
                        #ff4444 ${warnMinPercent}%,
                        #ffa500 ${warnMinPercent}%,
                        #ffa500 ${goodMinPercent}%,
                        #22c55e ${goodMinPercent}%,
                        #22c55e ${goodMaxPercent}%,
                        #ffa500 ${goodMaxPercent}%,
                        #ffa500 ${warnMaxPercent}%,
                        #ff4444 ${warnMaxPercent}%,
                        #ff4444 100%
                    )`
                }} />

                {/* Thumb handles */}
                <div
                    style={thumbStyle(warnMinPercent, 0)}
                    onMouseDown={(e) => handleMouseDown(0, e)}
                />
                <div
                    style={thumbStyle(goodMinPercent, 1)}
                    onMouseDown={(e) => handleMouseDown(1, e)}
                />
                <div
                    style={thumbStyle(goodMaxPercent, 2)}
                    onMouseDown={(e) => handleMouseDown(2, e)}
                />
                <div
                    style={thumbStyle(warnMaxPercent, 3)}
                    onMouseDown={(e) => handleMouseDown(3, e)}
                />
            </div>

            {/* Value labels */}
            <div className="slider-values">
                <span style={{ color: '#ffa500' }}>{warnMin}</span>
                <span style={{ color: '#22c55e' }}>{goodMin}</span>
                <span style={{ color: '#22c55e' }}>{goodMax}</span>
                <span style={{ color: '#ffa500' }}>{warnMax}</span>
            </div>
        </div>
    );
}