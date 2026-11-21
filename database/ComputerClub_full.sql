--
-- PostgreSQL database dump
--

\restrict thNWfWceM8wafKdnPOFR0CjbuqrcQUyzO3XQ21Pgv91iGoWOGi8RW8uyWwwhUFg

-- Dumped from database version 18.6
-- Dumped by pg_dump version 18.6

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_table_access_method = heap;

--
-- Name: bookings; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.bookings (
    id integer NOT NULL,
    clientid integer NOT NULL,
    computernumber integer NOT NULL,
    plannedstart timestamp without time zone NOT NULL,
    durationminutes integer NOT NULL,
    status text DEFAULT 'active'::text NOT NULL,
    createdat timestamp without time zone NOT NULL
);


--
-- Name: bookings_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.bookings_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: bookings_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.bookings_id_seq OWNED BY public.bookings.id;


--
-- Name: clients; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.clients (
    id integer NOT NULL,
    fullname text NOT NULL,
    phone text NOT NULL,
    totalhours double precision DEFAULT 0 NOT NULL,
    totalspent numeric(12,2) DEFAULT 0 NOT NULL,
    discountpercent integer DEFAULT 0 NOT NULL
);


--
-- Name: clients_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.clients_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: clients_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.clients_id_seq OWNED BY public.clients.id;


--
-- Name: computers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.computers (
    number integer NOT NULL,
    name text NOT NULL,
    isactive boolean DEFAULT true NOT NULL
);


--
-- Name: sessions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sessions (
    id integer NOT NULL,
    clientid integer NOT NULL,
    computernumber integer NOT NULL,
    starttime timestamp without time zone NOT NULL,
    plannedendtime timestamp without time zone NOT NULL,
    endtime timestamp without time zone,
    durationminutes double precision DEFAULT 0 NOT NULL,
    totalcost numeric(12,2) DEFAULT 0 NOT NULL,
    tariffperhour numeric(12,2) DEFAULT 0 NOT NULL,
    onetimediscountpercent numeric(5,2) DEFAULT 0 NOT NULL,
    personaldiscountpercent numeric(5,2) DEFAULT 0 NOT NULL,
    iscompleted boolean DEFAULT false NOT NULL,
    bookingid integer,
    paidat timestamp without time zone,
    receiptclientname text,
    CONSTRAINT sessions_payment_completed CHECK (((paidat IS NULL) OR iscompleted))
);


--
-- Name: sessions_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.sessions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: sessions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.sessions_id_seq OWNED BY public.sessions.id;


--
-- Name: settings; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.settings (
    settingkey text NOT NULL,
    value text NOT NULL
);


--
-- Name: users; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.users (
    id integer NOT NULL,
    username text NOT NULL,
    password text NOT NULL,
    role text NOT NULL
);


--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: bookings id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bookings ALTER COLUMN id SET DEFAULT nextval('public.bookings_id_seq'::regclass);


--
-- Name: clients id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.clients ALTER COLUMN id SET DEFAULT nextval('public.clients_id_seq'::regclass);


--
-- Name: sessions id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sessions ALTER COLUMN id SET DEFAULT nextval('public.sessions_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Data for Name: bookings; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.bookings (id, clientid, computernumber, plannedstart, durationminutes, status, createdat) FROM stdin;
2	5	4	2026-10-17 22:33:40.794048	90	active	2026-10-17 21:33:40.794048
3	7	6	2026-10-18 18:00:00	120	active	2026-10-17 21:33:40.794048
4	8	7	2026-10-18 19:30:00	90	active	2026-10-17 21:33:40.794048
5	9	8	2026-10-19 15:00:00	180	active	2026-10-17 21:33:40.794048
6	10	9	2026-10-20 17:00:00	120	active	2026-10-17 21:33:40.794048
8	15	1	2026-11-16 01:00:00	1	active	2026-11-15 00:47:27.167627
\.


--
-- Data for Name: clients; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.clients (id, fullname, phone, totalhours, totalspent, discountpercent) FROM stdin;
2	Тестовый клиент 2	79000000002	0	0.00	0
4	Тестовый клиент 4	79000000004	5.25	787.50	0
5	Тестовый клиент 5	79000000005	5.25	787.50	0
7	Тестовый клиент 7	79000000007	10.5	1545.00	5
8	Тестовый клиент 8	79000000008	10.5	1545.00	5
9	Тестовый клиент 9	79000000009	21	2940.00	10
10	Тестовый клиент 10	79000000010	21	2940.00	10
11	Тестовый клиент 11	79000000011	21	2962.50	10
13	Тестовый клиент 13	79000000013	30	4125.00	15
14	Тестовый клиент 14	79000000014	30	4117.50	15
6	Тестовый клиент 6	79000000006	14.65	2289.16	5
12	Тестовый клиент 12	79000000012	31.733333333333334	4419.65	15
15	Тестовый клиент 15	79000000015	0.03333333333333333	6.66	0
3	Тестовый клиент 3	79000000003	9.25	1547.50	0
1	Тестовый клиент 1	79000000001	1.0499999999999998	209.16	0
\.


--
-- Data for Name: computers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.computers (number, name, isactive) FROM stdin;
1	ПК 1	t
2	ПК 2	t
3	ПК 3	t
4	ПК 4	t
5	ПК 5	t
6	ПК 6	t
7	ПК 7	t
8	ПК 8	t
9	ПК 9	t
10	ПК 10	t
11	ПК 11	t
12	ПК 12	t
\.


--
-- Data for Name: sessions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sessions (id, clientid, computernumber, starttime, plannedendtime, endtime, durationminutes, totalcost, tariffperhour, onetimediscountpercent, personaldiscountpercent, iscompleted, bookingid, paidat, receiptclientname) FROM stdin;
54	15	1	2026-11-15 00:47:14.829022	2026-11-15 01:47:14.829022	2026-11-15 00:47:30.070871	1	3.33	200.00	0.00	0.00	t	\N	2026-11-15 00:47:31.032369	Тестовый клиент 15
55	15	1	2026-11-15 00:47:37.5121	2026-11-15 00:48:37.5121	2026-11-15 00:48:37.5121	1	3.33	200.00	0.00	0.00	t	\N	2026-11-15 00:48:52.250711	Тестовый клиент 15
53	1	12	2026-10-17 21:41:07.637397	2026-10-17 22:41:07.637397	2026-10-17 22:41:07.637397	60	200.00	200.00	0.00	0.00	t	\N	2026-11-15 00:49:20.378627	Тестовый клиент 1
50	3	1	2026-10-17 20:58:40.794048	2026-10-18 00:58:40.794048	2026-10-18 00:58:40.794048	240	760.00	200.00	5.00	0.00	t	\N	2026-11-15 00:49:24.57414	Тестовый клиент 3
56	1	1	2026-11-16 18:35:07.579174	2026-11-16 18:36:07.579174	2026-11-16 18:36:07.579174	1	3.33	200.00	0.00	0.00	t	\N	2026-11-16 18:36:13.57763	Тестовый клиент 1
57	1	1	2026-11-16 19:19:49.016646	2026-11-16 19:20:49.016646	2026-11-16 19:20:49.016646	1	3.33	200.00	0.00	0.00	t	\N	2026-11-16 19:24:47.47119	Тестовый клиент 1
1	1	1	2026-09-06 14:37:07.722175	2026-09-06 16:07:07.722175	2026-09-06 14:37:32.616487	1	2.50	150.00	0.00	0.00	t	\N	2026-09-06 14:37:32.616487	Тестовый клиент 1
5	3	1	2026-10-12 10:00:00	2026-10-12 12:00:00	2026-10-12 12:00:00	120	300.00	150.00	0.00	0.00	t	\N	2026-10-12 12:00:00	Тестовый клиент 3
4	3	1	2026-10-09 10:00:00	2026-10-09 11:30:00	2026-10-09 11:30:00	90	225.00	150.00	0.00	0.00	t	\N	2026-10-09 11:30:00	Тестовый клиент 3
3	3	1	2026-10-06 10:00:00	2026-10-06 11:00:00	2026-10-06 11:00:00	60	150.00	150.00	0.00	0.00	t	\N	2026-10-06 11:00:00	Тестовый клиент 3
2	3	1	2026-10-03 10:00:00	2026-10-03 10:45:00	2026-10-03 10:45:00	45	112.50	150.00	0.00	0.00	t	\N	2026-10-03 10:45:00	Тестовый клиент 3
9	4	2	2026-10-13 10:00:00	2026-10-13 12:00:00	2026-10-13 12:00:00	120	300.00	150.00	0.00	0.00	t	\N	2026-10-13 12:00:00	Тестовый клиент 4
8	4	2	2026-10-10 10:00:00	2026-10-10 11:30:00	2026-10-10 11:30:00	90	225.00	150.00	0.00	0.00	t	\N	2026-10-10 11:30:00	Тестовый клиент 4
7	4	2	2026-10-07 10:00:00	2026-10-07 11:00:00	2026-10-07 11:00:00	60	150.00	150.00	0.00	0.00	t	\N	2026-10-07 11:00:00	Тестовый клиент 4
6	4	2	2026-10-04 10:00:00	2026-10-04 10:45:00	2026-10-04 10:45:00	45	112.50	150.00	0.00	0.00	t	\N	2026-10-04 10:45:00	Тестовый клиент 4
13	5	3	2026-10-14 10:00:00	2026-10-14 12:00:00	2026-10-14 12:00:00	120	300.00	150.00	0.00	0.00	t	\N	2026-10-14 12:00:00	Тестовый клиент 5
12	5	3	2026-10-11 10:00:00	2026-10-11 11:30:00	2026-10-11 11:30:00	90	225.00	150.00	0.00	0.00	t	\N	2026-10-11 11:30:00	Тестовый клиент 5
11	5	3	2026-10-08 10:00:00	2026-10-08 11:00:00	2026-10-08 11:00:00	60	150.00	150.00	0.00	0.00	t	\N	2026-10-08 11:00:00	Тестовый клиент 5
10	5	3	2026-10-05 10:00:00	2026-10-05 10:45:00	2026-10-05 10:45:00	45	112.50	150.00	0.00	0.00	t	\N	2026-10-05 10:45:00	Тестовый клиент 5
21	7	5	2026-10-16 10:00:00	2026-10-16 14:00:00	2026-10-16 14:00:00	240	570.00	150.00	5.00	0.00	t	\N	2026-10-16 14:00:00	Тестовый клиент 7
20	7	5	2026-10-13 10:00:00	2026-10-13 13:00:00	2026-10-13 13:00:00	180	450.00	150.00	0.00	0.00	t	\N	2026-10-13 13:00:00	Тестовый клиент 7
19	7	5	2026-10-10 10:00:00	2026-10-10 12:00:00	2026-10-10 12:00:00	120	300.00	150.00	0.00	0.00	t	\N	2026-10-10 12:00:00	Тестовый клиент 7
18	7	5	2026-10-07 10:00:00	2026-10-07 11:30:00	2026-10-07 11:30:00	90	225.00	150.00	0.00	0.00	t	\N	2026-10-07 11:30:00	Тестовый клиент 7
25	8	6	2026-10-14 10:00:00	2026-10-14 13:00:00	2026-10-14 13:00:00	180	450.00	150.00	0.00	0.00	t	\N	2026-10-14 13:00:00	Тестовый клиент 8
24	8	6	2026-10-11 10:00:00	2026-10-11 12:00:00	2026-10-11 12:00:00	120	300.00	150.00	0.00	0.00	t	\N	2026-10-11 12:00:00	Тестовый клиент 8
23	8	6	2026-10-08 10:00:00	2026-10-08 11:30:00	2026-10-08 11:30:00	90	225.00	150.00	0.00	0.00	t	\N	2026-10-08 11:30:00	Тестовый клиент 8
22	8	6	2026-10-03 10:00:00	2026-10-03 14:00:00	2026-10-03 14:00:00	240	570.00	150.00	5.00	0.00	t	\N	2026-10-03 14:00:00	Тестовый клиент 8
29	9	7	2026-10-15 10:00:00	2026-10-15 16:00:00	2026-10-15 16:00:00	360	810.00	150.00	5.00	5.00	t	\N	2026-10-15 16:00:00	Тестовый клиент 9
28	9	7	2026-10-12 10:00:00	2026-10-12 14:00:00	2026-10-12 14:00:00	240	540.00	150.00	5.00	5.00	t	\N	2026-10-12 14:00:00	Тестовый клиент 9
27	9	7	2026-10-09 10:00:00	2026-10-09 13:00:00	2026-10-09 13:00:00	180	450.00	150.00	0.00	0.00	t	\N	2026-10-09 13:00:00	Тестовый клиент 9
26	9	7	2026-10-04 10:00:00	2026-10-04 18:00:00	2026-10-04 18:00:00	480	1140.00	150.00	5.00	0.00	t	\N	2026-10-04 18:00:00	Тестовый клиент 9
33	10	8	2026-10-16 10:00:00	2026-10-16 16:00:00	2026-10-16 16:00:00	360	810.00	150.00	5.00	5.00	t	\N	2026-10-16 16:00:00	Тестовый клиент 10
32	10	8	2026-10-13 10:00:00	2026-10-13 14:00:00	2026-10-13 14:00:00	240	540.00	150.00	5.00	5.00	t	\N	2026-10-13 14:00:00	Тестовый клиент 10
31	10	8	2026-10-10 10:00:00	2026-10-10 13:00:00	2026-10-10 13:00:00	180	450.00	150.00	0.00	0.00	t	\N	2026-10-10 13:00:00	Тестовый клиент 10
30	10	8	2026-10-05 10:00:00	2026-10-05 18:00:00	2026-10-05 18:00:00	480	1140.00	150.00	5.00	0.00	t	\N	2026-10-05 18:00:00	Тестовый клиент 10
37	11	9	2026-10-14 10:00:00	2026-10-14 14:00:00	2026-10-14 14:00:00	240	540.00	150.00	5.00	5.00	t	\N	2026-10-14 14:00:00	Тестовый клиент 11
36	11	9	2026-10-11 10:00:00	2026-10-11 13:00:00	2026-10-11 13:00:00	180	427.50	150.00	0.00	5.00	t	\N	2026-10-11 13:00:00	Тестовый клиент 11
35	11	9	2026-10-06 10:00:00	2026-10-06 18:00:00	2026-10-06 18:00:00	480	1140.00	150.00	5.00	0.00	t	\N	2026-10-06 18:00:00	Тестовый клиент 11
34	11	9	2026-10-03 10:00:00	2026-10-03 16:00:00	2026-10-03 16:00:00	360	855.00	150.00	5.00	0.00	t	\N	2026-10-03 16:00:00	Тестовый клиент 11
45	13	11	2026-10-16 10:00:00	2026-10-16 17:00:00	2026-10-16 17:00:00	420	892.50	150.00	5.00	10.00	t	\N	2026-10-16 17:00:00	Тестовый клиент 13
44	13	11	2026-10-13 10:00:00	2026-10-13 16:00:00	2026-10-13 16:00:00	360	810.00	150.00	5.00	5.00	t	\N	2026-10-13 16:00:00	Тестовый клиент 13
43	13	11	2026-10-08 10:00:00	2026-10-08 19:00:00	2026-10-08 19:00:00	540	1282.50	150.00	5.00	0.00	t	\N	2026-10-08 19:00:00	Тестовый клиент 13
42	13	11	2026-10-05 10:00:00	2026-10-05 18:00:00	2026-10-05 18:00:00	480	1140.00	150.00	5.00	0.00	t	\N	2026-10-05 18:00:00	Тестовый клиент 13
49	14	12	2026-10-14 10:00:00	2026-10-14 16:00:00	2026-10-14 16:00:00	360	765.00	150.00	5.00	10.00	t	\N	2026-10-14 16:00:00	Тестовый клиент 14
48	14	12	2026-10-09 10:00:00	2026-10-09 19:00:00	2026-10-09 19:00:00	540	1215.00	150.00	5.00	5.00	t	\N	2026-10-09 19:00:00	Тестовый клиент 14
47	14	12	2026-10-06 10:00:00	2026-10-06 18:00:00	2026-10-06 18:00:00	480	1140.00	150.00	5.00	0.00	t	\N	2026-10-06 18:00:00	Тестовый клиент 14
46	14	12	2026-10-03 10:00:00	2026-10-03 17:00:00	2026-10-03 17:00:00	420	997.50	150.00	5.00	0.00	t	\N	2026-10-03 17:00:00	Тестовый клиент 14
51	6	3	2026-10-17 18:03:40.794048	2026-10-18 00:03:40.794048	2026-10-17 22:11:43.825861	249	744.16	200.00	5.00	5.00	t	\N	2026-10-17 22:11:43.825861	Тестовый клиент 6
17	6	4	2026-10-15 10:00:00	2026-10-15 14:00:00	2026-10-15 14:00:00	240	570.00	150.00	5.00	0.00	t	\N	2026-10-15 14:00:00	Тестовый клиент 6
16	6	4	2026-10-12 10:00:00	2026-10-12 13:00:00	2026-10-12 13:00:00	180	450.00	150.00	0.00	0.00	t	\N	2026-10-12 13:00:00	Тестовый клиент 6
15	6	4	2026-10-09 10:00:00	2026-10-09 12:00:00	2026-10-09 12:00:00	120	300.00	150.00	0.00	0.00	t	\N	2026-10-09 12:00:00	Тестовый клиент 6
14	6	4	2026-10-06 10:00:00	2026-10-06 11:30:00	2026-10-06 11:30:00	90	225.00	150.00	0.00	0.00	t	\N	2026-10-06 11:30:00	Тестовый клиент 6
52	12	5	2026-10-17 20:28:40.794048	2026-10-17 23:28:40.794048	2026-10-17 22:12:40.419677	104	294.65	200.00	0.00	15.00	t	\N	2026-10-17 22:12:40.419677	Тестовый клиент 12
41	12	10	2026-10-15 10:00:00	2026-10-15 17:00:00	2026-10-15 17:00:00	420	892.50	150.00	5.00	10.00	t	\N	2026-10-15 17:00:00	Тестовый клиент 12
40	12	10	2026-10-12 10:00:00	2026-10-12 16:00:00	2026-10-12 16:00:00	360	810.00	150.00	5.00	5.00	t	\N	2026-10-12 16:00:00	Тестовый клиент 12
39	12	10	2026-10-07 10:00:00	2026-10-07 19:00:00	2026-10-07 19:00:00	540	1282.50	150.00	5.00	0.00	t	\N	2026-10-07 19:00:00	Тестовый клиент 12
38	12	10	2026-10-04 10:00:00	2026-10-04 18:00:00	2026-10-04 18:00:00	480	1140.00	150.00	5.00	0.00	t	\N	2026-10-04 18:00:00	Тестовый клиент 12
\.


--
-- Data for Name: settings; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.settings (settingkey, value) FROM stdin;
TariffPerHour	200
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.users (id, username, password, role) FROM stdin;
1	demo_user_1		manager
2	demo_user_2		manager
3	admin	admin123	admin
4	manager	manager123	manager
\.


--
-- Name: bookings_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.bookings_id_seq', 8, true);


--
-- Name: clients_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.clients_id_seq', 15, true);


--
-- Name: sessions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.sessions_id_seq', 57, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.users_id_seq', 4, true);


--
-- Name: bookings bookings_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_pkey PRIMARY KEY (id);


--
-- Name: clients clients_phone_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.clients
    ADD CONSTRAINT clients_phone_key UNIQUE (phone);


--
-- Name: clients clients_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.clients
    ADD CONSTRAINT clients_pkey PRIMARY KEY (id);


--
-- Name: computers computers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.computers
    ADD CONSTRAINT computers_pkey PRIMARY KEY (number);


--
-- Name: sessions sessions_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT sessions_pkey PRIMARY KEY (id);


--
-- Name: settings settings_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.settings
    ADD CONSTRAINT settings_pkey PRIMARY KEY (settingkey);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- Name: sessions_paid_idx; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX sessions_paid_idx ON public.sessions USING btree (paidat) WHERE (paidat IS NOT NULL);


--
-- Name: sessions_unpaid_idx; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX sessions_unpaid_idx ON public.sessions USING btree (endtime, id) WHERE (iscompleted AND (paidat IS NULL));


--
-- Name: bookings bookings_clientid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_clientid_fkey FOREIGN KEY (clientid) REFERENCES public.clients(id) ON DELETE CASCADE;


--
-- Name: bookings bookings_computernumber_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_computernumber_fkey FOREIGN KEY (computernumber) REFERENCES public.computers(number);


--
-- Name: sessions sessions_bookingid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT sessions_bookingid_fkey FOREIGN KEY (bookingid) REFERENCES public.bookings(id) ON DELETE SET NULL;


--
-- Name: sessions sessions_client_history_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT sessions_client_history_fk FOREIGN KEY (clientid) REFERENCES public.clients(id) ON DELETE RESTRICT;


--
-- Name: sessions sessions_computernumber_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sessions
    ADD CONSTRAINT sessions_computernumber_fkey FOREIGN KEY (computernumber) REFERENCES public.computers(number);


--
-- PostgreSQL database dump complete
--

\unrestrict thNWfWceM8wafKdnPOFR0CjbuqrcQUyzO3XQ21Pgv91iGoWOGi8RW8uyWwwhUFg
