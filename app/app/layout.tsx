import type { Metadata } from "next";
import { Geist, Geist_Mono, Inconsolata } from "next/font/google";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

const inconsolata = Inconsolata({
  variable: "--font-inconsolata",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "PaceLab",
  description: "Effort-adjusted performance metrics for cyclists and runners. Raw pace lies. Effort Doesn't.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        className={`${geistSans.variable} ${geistMono.variable} ${inconsolata.variable} antialiased`}
      >
        {children}
      </body>
    </html>
  );
}
