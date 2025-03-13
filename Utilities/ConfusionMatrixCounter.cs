using AutoScanFQCTest.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest.Utilities
{
	public class ConfusionMatrixCounter
	{
		private int mTruePositiveCount;
		private int mFalsePositiveCount;
		private int mTrueNegativeCount;
		private int mFalseNegativeCount;

		public int TruePositiveCount
		{
			get { return mTruePositiveCount; }
		}

		public int FalsePositiveCount
		{
			get { return mFalsePositiveCount; }
		}

		public int TrueNegativeCount
		{
			get { return mTrueNegativeCount; }
		}

		public int FalseNegativeCount
		{
			get { return mFalseNegativeCount; }
		}

		public ConfusionMatrixCounter()
		{
			mTruePositiveCount = 0;
			mFalsePositiveCount = 0;
			mTrueNegativeCount = 0;
			mFalseNegativeCount = 0;
		}

		public void UpdateMatrixCount(bool predict, bool actual)
		{
			if(predict && actual)
			{
				mTruePositiveCount++;
			}
			else if(predict && !actual)
			{
				mFalsePositiveCount++;
			}
			else if(!predict && actual)
			{
				mTrueNegativeCount++;
			}
			else if(!predict && !actual)
			{
				mFalseNegativeCount++;
			}
		}

		public float CalculateAccuracy()
		{
			return 1.0f * (mTruePositiveCount + mTrueNegativeCount) / (mTruePositiveCount + mTrueNegativeCount + mFalsePositiveCount + mFalseNegativeCount);
		}

		public float CalculatePrecision()
		{
			return 1.0f * mTruePositiveCount / (mTruePositiveCount + mFalsePositiveCount);
		}

		public float CalculateRecall()
		{
			return 1.0f * mTruePositiveCount / (mTruePositiveCount + mFalseNegativeCount);
		}

		public float CalculateF1Score()
		{
			var precision = CalculatePrecision();
			var recall = CalculateRecall();

			return 2.0f * (precision * recall) / (precision + recall);
		}

		public float CalculateCurrentAccuracy(List<AVITrayInfo> predicted, List<AVITrayInfo> rechecked)
		{
			return 1.0f;
		}
	}
}