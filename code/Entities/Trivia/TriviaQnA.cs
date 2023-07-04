namespace Home.Games.Trivia;

public struct AnswerStruct
{
	public enum OptionEnum
	{
		UnSelected,
		A,
		B,
		C,
		D
	}

	/// <summary>
	/// The answer displayed to contestants
	/// </summary>
	public string Answer { get; set; } = "";

	/// <summary>
	/// Is this the correct answer or amongst the correct answers (if multi-choiced)
	/// </summary>
	public bool IsCorrect { get; set; } = false;

	/// <summary>
	/// The option this answer will fall under
	/// </summary>
	public OptionEnum Option { get; set; }

	/// <summary>
	/// Creates an answer for the question
	/// </summary>
	/// <param name="text">The string displayed to players</param>
	/// <param name="option">Which option does the answer go under</param>
	/// <param name="isCorrect">Is this answer correct or amongst the ones that is correct</param>
	public AnswerStruct(string text, OptionEnum option, bool isCorrect = false)
	{
		Answer = text;
		IsCorrect = isCorrect;
		Option = option;
	}
}

public struct QuestionStruct
{
	public enum SubjectEnum
	{
		Undefined,
		Gaming,
		Wildlife,
		Nature,
		History,
		Science,
		Phobias
	}

	public enum TypeEnum
	{
		Standard,		//4 possible answers, only 1 is correct
		TrueOrFalse,	//50/50, only contains 2 possible answers being true or false
		MultiChoice		//Has more than 1 correct answer
	}

	public enum DifficultyEnum
	{
		Trivial,		//Very basic difficulty, everyone should know this
		Easy,			//everyone won't know about but majority do
		Medium,			//Half in half, some would know and some don't
		Hard,			//Few will know the answer
		HowDoYouKnow,	//Very little know the answer, how did they know?
	}

	/// <summary>
	/// The question displayed to contestants
	/// </summary>
	public string Question { get; set; } = "";

	/// <summary>
	/// The subject the question is in
	/// </summary>
	public SubjectEnum Subject { get; set; } = SubjectEnum.Undefined;

	/// <summary>
	/// The question type
	/// </summary>
	public TypeEnum QuestionType { get; set; } = TypeEnum.Standard;

	/// <summary>
	/// A list of possible answers
	/// </summary>
	public AnswerStruct[] Answers { get; set; }
	
	public QuestionStruct()
	{

	}
}
public static class QnASheet
{
	//Question template
	/*
		new QuestionStruct()
		{
			Question = "",
			Subject = enum,
			QuestionType = Qtype,
			Answers = new AnswerStruct[]
			{
				new AnswerStruct("", option),
				new AnswerStruct("", option),
				new AnswerStruct("", option),
				new AnswerStruct("", option),
			}
		}
	*/

	public static QuestionStruct[] Questions = new QuestionStruct[]
	{
		//Gaming
		#region
		new QuestionStruct()
		{
			Question = "Which person did not program DOOM (1993)",
			Subject = QuestionStruct.SubjectEnum.Gaming,
			QuestionType = QuestionStruct.TypeEnum.Standard,
			Answers = new AnswerStruct[]
			{
				new AnswerStruct("John Carmack", AnswerStruct.OptionEnum.A),
				new AnswerStruct("Jonh Romero", AnswerStruct.OptionEnum.B),
				new AnswerStruct("Dave Taylor", AnswerStruct.OptionEnum.C),
				new AnswerStruct("Adrian Carmack", AnswerStruct.OptionEnum.D, true)
			}
		},

		new QuestionStruct()
		{
			Question = "Who founded Valve Corporation alongside Gabe Newell",
			Subject = QuestionStruct.SubjectEnum.Gaming,
			QuestionType = QuestionStruct.TypeEnum.Standard,
			Answers = new AnswerStruct[]
			{
				new AnswerStruct("Andrew Kim", AnswerStruct.OptionEnum.A),
				new AnswerStruct("Erik Johnson", AnswerStruct.OptionEnum.B),
				new AnswerStruct("Mike Harrington", AnswerStruct.OptionEnum.C, true),
				new AnswerStruct("David Speyrer", AnswerStruct.OptionEnum.D),
			}
		},
		#endregion

		//Wildlife
		#region
		new QuestionStruct()
		{
			Question = "Do polar bears have black skin underneath its fur",
			Subject = QuestionStruct.SubjectEnum.Wildlife,
			QuestionType = QuestionStruct.TypeEnum.TrueOrFalse,
			Answers = new AnswerStruct[]
			{
				new AnswerStruct("TRUE", AnswerStruct.OptionEnum.A),
				new AnswerStruct("FALSE", AnswerStruct.OptionEnum.B),
			}
		},

		new QuestionStruct()
		{
			Question = "What sounds do giraffes make",
			Subject = QuestionStruct.SubjectEnum.Wildlife,
			QuestionType = QuestionStruct.TypeEnum.MultiChoice,
			Answers = new AnswerStruct[]
			{
				new AnswerStruct("Snorting", AnswerStruct.OptionEnum.A, true),
				new AnswerStruct("Growling", AnswerStruct.OptionEnum.B),
				new AnswerStruct("Hissing", AnswerStruct.OptionEnum.C, true),
				new AnswerStruct("Squealing", AnswerStruct.OptionEnum.D),
			}
		},
		#endregion

		//Nature
		#region
		new QuestionStruct()
		{
			Question = "How long can oak trees live for",
			Subject = QuestionStruct.SubjectEnum.Nature,
			QuestionType = QuestionStruct.TypeEnum.Standard,
			Answers = new AnswerStruct[]
			{
				new AnswerStruct("1,000", AnswerStruct.OptionEnum.A, true),
				new AnswerStruct("100", AnswerStruct.OptionEnum.B),
				new AnswerStruct("10,000", AnswerStruct.OptionEnum.C),
				new AnswerStruct("10", AnswerStruct.OptionEnum.D),
			}
		},

		new QuestionStruct()
		{
			Question = "Which is not a type of plant",
			Subject = QuestionStruct.SubjectEnum.Nature,
			QuestionType = QuestionStruct.TypeEnum.Standard,
			Answers = new AnswerStruct[]
			{
				new AnswerStruct("Moss", AnswerStruct.OptionEnum.A),
				new AnswerStruct("Mushroom", AnswerStruct.OptionEnum.B, true),
				new AnswerStruct("Water lily", AnswerStruct.OptionEnum.C),
				new AnswerStruct("Potato", AnswerStruct.OptionEnum.D),
			}
		},
		#endregion
	};

	public static List<QuestionStruct> QuestionsArray = Questions.ToList();

	public static void ResetQuestions() => QuestionsArray = Questions.ToList();

	public static QuestionStruct TakeQuestion()
	{
		QuestionStruct question = QuestionsArray[Game.Random.Int( 0, QuestionsArray.Count - 1 )];

		QuestionsArray.Remove( question );

		return question;
	}
}
