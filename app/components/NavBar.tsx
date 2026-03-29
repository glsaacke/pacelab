import Link from 'next/link';
import '@/app/styles/components/NavBar.css';

const NavBar = () => {
    return ( 
        <div className="navbar-container">
            <Link href="/" className="navbar-logo">PACELAB</Link>
            <Link href="/login" className="navbar-login">Login</Link>
        </div>
     );
}
 
export default NavBar;