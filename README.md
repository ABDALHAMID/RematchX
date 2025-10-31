# RematchX ⚽️ — Web3 Arcade Football (3v3 / 4v4 / 5v5)

RematchX is a fast, arcade-style football game for **mobile & PC** where players **own** their footballer cards as **NFTs**, play **free** (Casual & Ranked with Tickets), and optionally compete in **Stakes** (entry-fee prize pools). Cards can be **bought, sold, or rented**; even a **Common** can evolve to **Mythic** via **Ascension** (play + materials). Built on **Hedera** for instant, low-fee, transparent operations.

---

## 🎥 Demo Videos
> Quick looks at early gameplay and systems.

[![Demo 1](https://img.youtube.com/vi/myHo7sBQjFY/0.jpg)](https://www.youtube.com/watch?v=myHo7sBQjFY)
[![Demo 2](https://img.youtube.com/vi/81rm4xntqAw/0.jpg)](https://www.youtube.com/watch?v=81rm4xntqAw)
[![Demo 3](https://img.youtube.com/vi/3x3cXRfnVYY/0.jpg)](https://www.youtube.com/watch?v=3x3cXRfnVYY)

---

## 🌟 Core Features
- **Arcade Football**: 3–5 minute matches, 3v3/4v4/5v5, tight controls.
- **Own What You Play**: Player cards as **HIP-412 NFTs** (Hedera).
- **Fair F2P**: Casual (free) + Ranked (tickets from quests).
- **Optional Stakes**: Entry in **KICK** (fixed UI: **1 USD = 100 KICK**), prize pools, compliance-gated.
- **Gacha (Transparent Odds)**: Common → Rare → Epic → Legendary → Mythic, pity counters.
- **Ascension System**: Any Common can reach Mythic by filling **all 5 stat bars** + paying **KICK/GOAL/Shards**.
- **Rentals with Commission**: Lenders earn **KICK fees + % of renter’s GOAL** during rental.
- **Hedera-Native**: HTS tokens, HIP-412 NFTs, HCS audit trail.

---

## 🧱 Monorepo Structure (suggested)
```
rematchx/
├─ rematchx-client/        # Unity project (Android/iOS/PC)
├─ rematchx-backend/       # Node.js/Express + MongoDB APIs
└─ rematchx-contracts/     # Hedera EVM contracts (PrizePool, Rental, SeasonMinter)
```

---

## 🕹 Gameplay Summary
- **Stats** on each player NFT: **Speed, Endurance, Dribble, Shot, Defense**.
- **Queues**
  - **Casual (Free)** — no entry, no cash payout; XP, cosmetics, Training Points (TP).
  - **Ranked (Tickets)** — 1 Ticket to queue; Tickets from quests (**+3/day**, cap **5/day**, hold **20**).
  - **Stakes (Entry Fee)** — KICK entry → automatic prize pool payout (e.g., 3v3: **20 KICK/player → pool 120 → 80/20 split**).
- **Free → Stakes Path**: **10 Tickets → 100 KICK (locked)** usable **only** for entry fees.

---

## 💎 Gacha & Ascension
- **Odds (public):** Common **65%**, Rare **22%**, Epic **9%**, Legendary **3.5%**, Mythic **0.5%**.  
- **Pity:** ≥ **Epic** every **10** Standard pulls; ≥ **Legendary** every **50** total pulls.  
- **Duplicates → Shards** (Ascension material).  
- **Ascend Conditions:** fill all 5 stat bars to tier cap + **Integrity 100%** + costs.  
- **Example Ascension Costs:**
  - C→R: **400 KICK + 40 GOAL + 20 Shards**
  - R→E: **900 KICK + 90 GOAL + 60 Shards**
  - E→L: **2,000 KICK + 200 GOAL + 160 Shards**
  - L→M: **5,000 KICK + 500 GOAL + 400 Shards**
- **Rarity Caps (example per stat):** 60 / 68 / 76 / 84 / 92 (season soft cap +20% over base).

---

## 🪙 Tokenomics (Hedera HTS)
- **KICK (credits; UX fixed 1 USD = 100 KICK)**  
  Used for: entries, packs, rentals, training, repairs, fees.  
  *Non-P2P or restricted* for stability; under the hood funded by HBAR/USDC.
- **GOAL (reward/governance)**  
  Transferable. **1,000,000/season** emission (example) with caps.  
  Distribution: Matches **40%**, Tournaments **25%**, Quests **25%**, Staking **10%**.  
  **Daily cap:** 50 GOAL/account; Proof-of-Play weighted.

**Sinks:** training/repairs, club/league fees, listing/relist fees, skin crafting, name/banner changes.

---

## 🔁 Rentals (wNFT Model)
- **Escrow original NFT** → renter receives **non-transferable wNFT** (same stats).  
- **Pricing (defaults):** 5 KICK/match **or** 50 KICK/day; deposit 50 KICK.  
- **Owner Commission:** **10% of renter’s GOAL** (optional +5% net prize KICK).  
- **Auto-expiry:** by time or N matches; burn wNFT → unlock original → settle payments.

---

## 🧩 Tech Overview
- **Chain:** Hedera (HTS tokens, HIP-412 NFTs, HCS logs).  
- **Contracts:** `PrizePool`, `RentalManager` + `wPlayer`, `SeasonMinter`.  
- **Backend:** Node.js/Express + MongoDB (auth, MMR, inventory, rewards, rentals).  
- **Client:** Unity (Photon/Netcode), wallet connect (HashPack deep link), “No real-money mode” toggle for F2P.

---

## 🚀 Quick Start (Testnet)

### Prereqs
- **Node.js 20+**, **npm** or **pnpm**
- **MongoDB** (local or Atlas)
- **Unity 2022/2023 LTS**
- **HashPack** wallet (browser extension)

### 1) Smart Contracts (`rematchx-contracts/`)
```bash
cd rematchx-contracts
npm i
# Configure Hardhat for Hedera EVM / testnet RPC in hardhat.config.js
# Add .env
cp .env.example .env
# HEDERA_TESTNET_RPC=...
# HEDERA_ACCOUNT_ID=0.0.xxxx
# HEDERA_PRIVATE_KEY=302e0201...
npx hardhat compile
npx hardhat run scripts/deploy.ts --network hedera_testnet
```
_Outputs:_ contract addresses for **PrizePool**, **RentalManager**, **SeasonMinter**.

### 2) Backend (`rematchx-backend/`)
```bash
cd rematchx-backend
npm i
cp .env.example .env
# Required:
# PORT=8080
# MONGO_URI=mongodb+srv://...
# JWT_SECRET=change-me
# HEDERA_NETWORK=testnet
# HEDERA_ACCOUNT_ID=0.0.xxxx
# HEDERA_PRIVATE_KEY=302e0201...
# CONTRACT_PRIZE_POOL=0x...
# CONTRACT_RENTAL_MANAGER=0x...
# CONTRACT_SEASON_MINTER=0x...
npm run dev
```

### 3) Client (`rematchx-client/`)
- Open in **Unity**.
- Set API base URL in `Assets/Resources/config.json`:
```json
{
  "apiBaseUrl": "http://localhost:8080",
  "showRealMoneyUI": false
}
```
- Build **Android/iOS/PC** from Unity Build Settings.

> **Never** share **mainnet** private keys. Use **Testnet** only for demos.  
> Fund your Testnet account with tℏ via the **Hedera Faucet**.

---

## 🔌 Key API Endpoints (minimal)
```
POST /auth/login
GET  /profile
POST /match/create
POST /match/finish
POST /prize/start
POST /prize/settle
POST /tickets/claimDaily
POST /tickets/convert           # 10 tickets → 100 KICK (locked)
POST /rental/list               # list NFT for rent
POST /rental/rent               # start rental (escrow original → mint wNFT)
POST /rental/end                # forced end after expiry, settle payments
POST /nft/mintBase              # grant base NFT after training
GET  /economy/rates             # e.g., 1 USD = 100 KICK
```

---

## 🧪 Default Balancing (MVP)
- **Tickets:** +3/day (cap 5/day; hold 20)
- **Ticket → KICK:** 10 Tickets → **100 KICK (locked)**
- **Stakes (3v3):** 20 KICK/player → pool 120 → **80/20 split**
- **TP/day cap:** 120; **Integrity repair:** +20% for 60 KICK or 10 Shards
- **Ascension costs:** C→R 400K/40G/20S; R→E 900K/90G/60S; E→L 2000K/200G/160S; L→M 5000K/500G/400S
- **Rental:** 5 KICK/match or 50 KICK/day; deposit 50; **owner 10% GOAL commission**
- **GOAL emission:** 1,000,000/season; 50/day cap/account

---

## 🗺 Roadmap (12-Week MVP)
- **W1–2:** Core controls + Casual  
- **W3–4:** Backend base + Ranked (Tickets)  
- **W5–6:** HTS (KICK/GOAL) + `PrizePool` v1  
- **W7–8:** Marketplace + Rentals (wNFT + commission)  
- **W9:** Anti-cheat + compliance gates  
- **W10–11:** Balance + QA + economy caps  
- **W12:** Trailer + public Testnet demo + submission

---

## 🤝 Contributing
PRs welcome! Please open an issue first for major changes.  
Run linters/tests before pushing. Keep secrets out of git.

---

## 🔒 Security
If you discover a security issue, please email the maintainers directly.  
**Do not** open public issues for vulnerabilities.

---

## 📜 License
MIT © RematchX Contributors

---

## 👋 Contact
- Team: RematchX (Universiapolis – Agadir)  
