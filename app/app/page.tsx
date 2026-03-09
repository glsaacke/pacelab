import Link from 'next/link';
import './styles/page.css';

export default function Home() {
  return (
    <div className="home-container">
      <div className="home-section1">
        <div className="home-section1-background"></div>
        <div className="home-nav">
          <Link href="/">PACELAB</Link>
          <Link href="">Login</Link>
        </div>
        <div className="home-body">
          <h1>Raw pace lies. Effort Doesn't.</h1>
        </div>
      </div>
    </div>
  );
}
