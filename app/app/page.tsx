import Link from 'next/link';
import './styles/page.css';
import NavBar from '@/components/NavBar';

export default function Home() {
  return (
    <div className="home-container">
      <div className="home-section1">
        <div className="home-section1-background"></div>
        <NavBar />
        <div className="home-body">
          <h1>Raw pace lies. Effort Doesn&apos;t.</h1>
          <p className="home-subtitle">
            Environmental-adjusted pacing for runners and cyclists who train with purpose.
          </p>

          <div className="home-pace-demo">
            <p className="home-pace-label">Effort-normalized pace</p>
            <div className="home-pace-card">
              <span className="home-pace-day">Monday</span>
              <span className="home-pace-raw">8:10</span>
              <span className="home-pace-text">pace</span>
              <span className="home-pace-arrow">→</span>
              <span className="home-pace-adjusted">7:35</span>
              <span className="home-pace-text">effort-adjusted</span>
            </div>
            <div className="home-pace-card">
              <span className="home-pace-day">Friday</span>
              <span className="home-pace-raw">7:42</span>
              <span className="home-pace-text">pace</span>
              <span className="home-pace-arrow">→</span>
              <span className="home-pace-adjusted">7:14</span>
              <span className="home-pace-text">effort-adjusted</span>
            </div>
          </div>

          <Link href="/register" className="home-cta">START ANALYZING</Link>
        </div>
        <div className="home-learn-more">
            <span>Learn More</span>
            <svg width="20" height="12" viewBox="0 0 20 12" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1 1L10 10L19 1" stroke="white" strokeWidth="2" strokeLinecap="round"/>
            </svg>
          </div>
      </div>
    </div>
  );
}
